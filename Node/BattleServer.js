const WebSocket = require('ws');

class BattleServer{
    constructor(port){
        this.wss = new WebSocket.Server({port});
        this.clients = new Set();
        this.players = new Map();
        this.waitinPlayers = [];
        this.battles = new Map();
        this.setupServerEvents();
        console.log(`배틀 서버가 포트 ${port}에서 시작 되었습니다.`);
    }

    setupServerEvents()
    {
        this.wss.on('connection', (socket) => {
            this.clients.add(socket);
            const playerId = this.generatePlayerId();

            this.waitinPlayers.set(playerId, {
                socket : socket,
                id : playerId,
                name : `Player_${playerId,substr(-4)}`,
                hp : 100,
                maxHp : 100,
                inBattle : false,
                battleId : null
            });

            console.log(`플레이어 접속 : ${playerId} (총 ${this.clients.size} 명)`);

            this.sendToPlayer(playerId, {
                type : 'connected',
                playerId : playerId,
                playerData : this.players.get(playerId)
            })

            socket.on('message' , (message) =>
            {
                try
                {
                    const data = JSON.parse(message);
                    this.handleMessage(playerId, data);
                }
                catch(error)
                {
                    console.error('메세지 파싱 에러 :' , error);
                }
            });

            socket.on('close' , () =>
            {
                this.handleDisconnect(playerId);
            });

            socket.on('error' , (error) =>
            {
                console.error('소켓 에러' , error);
            });
        });
    }

    handleMessage(playerId, data)
    {
        console.log(`메세지 수신 [${playerId}]` , data.type);
        switch (data.type)
        {
            case 'findMatch' :
                this.handleFindMatch(playerId);
                break;
            case 'cancelMatch' :
                this.handleCancelMatch(playerId);
                break;
            case 'battleAction' :
                this.handelBattleAction(playerId, data.action);
                break;

            default :
                console.log(`알 수 없는 메세지 타입 : ${data.type}`);
        }
    }

    handleFindMatch(playerId)
    {
        const player = this.players.get(playerId);
        if (!player) return;

        if(player.inBattle){
            this.sendToPlayer(playerId, {
                type : 'error',
                message : '이미 배틀 중입니다.'
            });
            return;
        }

        if(this.waitinPlayers.includes(playerId))
        {
            this.sendToPlayer(playerId, {
                type : 'error',
                message : '이미 매칭 대기 중입니다.'
            });
            return;
        }
        console.log(`매칭 대기 추가 : ${playerId}`);
        this.waitinPlayers.push(playerId);

        this.sendToPlayer(playerId, {
            type : 'matchSearching',
            message : '상대를 찾는 중...'
        });
        
        this.tryMatchPlayers();
    }

    handleCancelMatch(playerId)
    {
        const index = this.waitinPlayers.indexOf(playerId);
        if(index > -1)
        {
            this.waitinPlayers.splice(index, 1);
            console.log(`매칭 취소 : ${playerId}`);

            this.sendToPlayer(playerId, {
                type : 'matchCanceled',
                message : '매칭이 취소되었습니다.'
            });
        }
    }

    tryMatchPlayers()
    {
        while ( this.waitinPlayers.length >= 2)
        {
            const player1Id = this.waitinPlayers.shift();
            const player2Id = this.waitinPlayers.shift();

            this.startBattle(player1Id, player2Id);
        }
    }

    startBattle(player1Id, player2Id)
    {
        const battleId = this.generatePlayerId();
        const player1 = this.players.get(player1Id);
        const player2 = this.players.get(player2Id);

        if (!player1 || !player2)
        {
            console.error('플레이어를 찾을 수 없습니다.');
            return;
        }

        player1.hp = player1.maxHp;
        player2.hp = player2.maxHp;
        player1.inBattle = true;
        player2.inBattle = true;
        player1.battleId = battleId;
        player2.battleId = battleId;

        const battle = {
            id : battleId,
            player1 : player1Id,
            player2 : player2Id,
            currentTurn : player1Id,
            turnCount : 1,
            player1LastAction : null,
            player2LastAction : null,
            isWaitingForAction : true
        };

        this.battles.set(battleId, battle);
        console.log(`배틀 시작 : ${battleId}`);
        console.log(`Player1 : ${player1Id} vs Player2 : ${player2Id}`);

        const battleStartMsg = {
            type : 'battleStart',
            battleId : battleId,
            opponent : null,
            yourTurn : null,
            player1 : {
                id : player1Id,
                name : player1.name,
                hp : player1.hp,
                maxHp : player1.maxHp
            },
            player2 : {
                id : player2Id,
                name : player2.name,
                hp : player2.hp,
                maxHp : player2.maxHp
            }
        };

        this.sendToPlayer(player1Id, {
            ...battleStartMsg,
            opponent : player2.name,
            yourTurn : true,
            isPlayer1 : true
        });

        this.sendToPlayer(player2Id, {
            ...battleStartMsg,
            opponent : player1.name,
            yourTurn : false,
            isPlayer1 : false
        });

    }

    handelBattleAction(playerId, action)
    {
        const player = this.players.get(playerId);
        if(!player || ~player.inBattle)
        {
            console.log (`배틀 중이 아닌 플레이어의 액션 : ${playerId}`);
            return;
        }

        const battle = this.battles.get(player.battleId);
        if(!battle)
        {
            console.log(`배틀을 찾을 수 없음 : ${player.battleId}`);
            return;
        }

        if(battle.currentTurn !== playerId)
        {
            this.sendToPlayer(playerId, {
                type : 'error',
                message : '당신의 턴이 아닙니다.'
            });
            return;
        }

        console.log(`배틀 액션 : ${playerId} -> ${action}`);

        if (battle.player1 === playerId)
        {
            battle.player1LastAction = action;
        }
        else
        {
            battle.player2LastAction = action;
        }

        this.processBattleAction(battle, playerId, action);
    }

    processBattleAction(battle, attackerId, action)
    {
        const attacker = this.players.get(attackerId);
        const defenderId = battle.player1 === attackerId ? battle.player2 : battle.player1;
        const defender = this.players.get(defenderId);

        let damage = 0;
        let actionText = '';

        switch(action)
        {
            case 'attack' :
                damage = Math.floor(Math.random() * 15) + 10;
                actionText = `${attacker.name}의 공격!`;
                break;

            case 'defend' :
                damage = 0;
                actionText = `${attacker.name}이(가) 방어했다!`;
                break;

            case 'skill' :
                damage = Math.floor(Math.random() * 25) + 20;
                actionText = `${attacker.name}의 필살기!`;

            default:
                damage = 0;
                actionText = `${attacker.name}이(가) 행동했다.`;
        }

        if(action !== 'defend')
        {
            defender.hp = Math.max(0, defender.hp - damage);
        }

        console.log(`${actionText} -> ${damage} 데미지!`);
        console.log(`${defender.name} -> ${defender.hp} / ${defender.maxHp}`);

        const actionResult = {
            type : 'battleAction',
            battleId : battle.id,
            attacker : attacker.name,
            action : action,
            damage : damage,
            actionText : actionText,
            player1Hp : this.players.get(battle.player1).hp,
            player2Hp : this.players.get(battle.player2).hp
        };

        this.sendToPlayer(battle.player1, actionResult);
        this.sendToPlayer(battle.player2, actionResult);

        if ( defender.hp <= 0)
        {
            this.endBattle(battle, attackerId);
            return;
        }

        battle.currentTurn = defenderId;
        battle.turnCount++;

        const nextTurnMsg = {
            type : 'nextTurn',
            battleId : battle.id,
            currentTurn : battle.currentTurn,
            turnCount : battle.turnCount
        };

        this.sendToPlayer(battle.player1, {
            ...nextTurnMsg,
            yourTurn : battle.currentTurn === battle.player1
        });

        this.sendToPlayer(battle.player2, {
            ...nextTurnMsg,
            yourTurn : battle.currentTurn === battle.player2
        });
    }

    endBattle(battle, winnerId)
    {
        const loserId = battle.player1 === winnerId ? battle.player2 : battle.player1;
        const winner = this.players.get(winnerId);
        const loser = this.players.get(loserId);

        console.log(`${winner.name} 숭리!`);

        const endMsg = {
            type : 'battleEnd',
            battleId : battle.id,
            winner : winner.name,
            winnerId : winnerId,
            loser : loser.name,
            loserId : loserId
        };

        this.sendToPlayer(winnerId , {
            ...endMsg,
            result : 'win',
            message : '승리했습니다.'
        });
        this.sendToPlayer(loserId , {
            ...endMsg,
            result : 'lose',
            message : '패배했습니다...'
        });

        winner.inBattle = false;
        winner.battleId = null;
        loser.inBattle = false;
        loser.battleId = null;

        this.battles.delete(battle.id);
    }

    handleDisconnect(playerId)
    {
        this.clients.delete(this.players.get(playerId) ?.socket);

        const waitingIndex = this.waitinPlayers.indexOf(playerId);
        if(waitingIndex > -1)
        {
            this.waitinPlayers.splice(waitingIndex, 1);
        }
        const player = this.players.get(playerId);

        if(player && player.inBattle)
        {
            const battle = this.battles.get(player.battleId);
            if(battle)
            {
                const opponentId = battle.player1 === playerId ? battle.player2 : battle.player1;

                this.sendToPlayer(opponentId, {
                    type : 'opponentDisconnected',
                    message : '상대방이 연결을 종료 했습니다. 당신이 승리 했습니다.'
                });

                const opponent = this.players.get(opponentId);
                if(opponent)
                {
                    opponent.inBattle = false;
                    opponent.battleId = null;
                }

                this.battles.delete(player.battleId);
            }
        }

        this.players.delete(playerId);
        console.log(`플레이어 퇴장 : ${playerId} (남은 인원 : ${this.clients.size} 명)`);
    }

    sendToPlayer(playerId, data)
    {
        const player = this.player.get(playerId);
        if(player && player.socket.readyState === WebSocket.OPEN)
        {
            player.socket.send(JSON.stringify(data));
        }
    }

    broadcast(data, excludePlayerId = null)
    {
        const message = JSON.stringify(data);
        this.players.forEach((player, id) => 
        {
            if(id !== excludePlayerId && player.socket.readyState === WebSocket.OPEN)
            {
                player.socket.send(message);
            }
        });
    }

    generatePlayerId()
    {
        return 'player_' + Math.random().toString(36).substring(2,9);
    }

    generateBattleId()
    {
        return 'battle_' + Math.random().toString(36).substring(2,9);
    }

}

const battleServer = new BattleServer(3001);