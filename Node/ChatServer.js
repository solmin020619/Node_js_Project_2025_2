const WebSocket = require('ws');
const iconv = require('iconv-lite');

class GameServer {

    constructor(port){
        this.wss = new WebSocket.Server({port});
        this.clients = new Set();
        this.players = new Map();
        this.SetupServerEvent();
        console.log(`게임 서버 포트 ${port}에서 시작 되었습니다 `);
    }

SetupServerEvent()
{
    this.wss.on('connection',(socket) => {
        this.clients.add(socket);
        const playerId = this.generatePlayerId();

        this.players.set(playerId,{
            socket : socket,
            position: {x:0,y:0,z:0},
            rotation: {x:0,y:0,z:0}
        });
        console.log(`클라이언트 접속! ID : ${playerId},현재 접속자 : ${this.clients.size}`);

        const welcomData = {
            type : 'connection',
            playerId : playerId,
            message : '서버에 연결되었습니다'
        };

        // 기존 플레이어들 정보를 새 플레이어에게 전송
        this.players.forEach((playerId,pid) => {
            if(pid != playerId){
                const joinsMsg = {
                    type : 'playerJoin',
                    playerId : pid,
                    position : player.position,
                    rotation : player.rotation
                };
                socket.send(JSON.stringify(joinsMsg));
                console.log(`기존 플레이어 정보 전송 : ${pid} -> ${playerId}`);
            }
        });

        socket.send(JSON.stringify(welcomData));

        socket.on('message',(message) =>{
            try
            {
                const data = JSON.parse(message);
                if(data.type === 'chat')
                {
                    console.log('수신된 메세지 :', data);

                    // 채팅 메세지 브로드캐스트(보내 사람 정보 포함)
                    this.broadcast({
                        type : 'chat',
                        playerId : playerId,
                        message : data.message
                    });
                }
                else if(data.type === 'positionUpdate')
                {
                    const player = this.players.get(playerId);
                    if (player)
                    {
                        if(data.position)   // 위치값 저장
                        {
                            player.position = data.position;
                        }
                        if(data.rotation)   // 회전값 저장
                        {
                            player.rotation = data.rotation;
                        }   
                    }

                    // 다른 플레이어들에게 브로드 케스트
                    const updateMsg = {
                        type : 'postionUpdate',
                        playerId : playerId,
                        position : player.position,
                        rotation : player.rotation
                    };
                    this.broadcast(updateMsg,socket);
                }
            }
            catch
            {
                console.error('메세지 파싱 에러 : ',error);
            }
        });

        socket.on('close',() =>{
            this.clients.delete(socket);
            this.players.delete(playerId);

            this.broadcast({
                type : 'playerDisconnect',
                playerId : playerId
            });

            console.log(`클라이언트 퇴장 ID ${playerId}, 현재 접속자 : ${this.clients.size}`);
        });

        socket.on('error',(error) =>{
            console.error('소켓 에러 : ',error);
        });
    });
}
    broadcast(data,excludeSocket = null)
    {
        const message = JSON.stringify(data);
        let sentCount = 0;

        this.clients.forEach(client =>
        {
            if(client !== excludeSocket && client.readyState === WebSocket.OPEN){
                client.send(message);
                sentCount++;
            }
        });

        if(data.type !== 'postionUpdate'){
            console.log(`브로드 케스트 완료 ${data.type} (${sentCount} 명에게 전송)`);
        }
    }

    generatePlayerId()
    {
        return 'player_' + Math.random().toString(36).substr(2,9);
    }
}

const gameServer = new GameServer(3000);