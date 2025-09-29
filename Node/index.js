const express = require('express');
const fs = require('fs');                               // 파일 시스템 해더에 추가
const playerRoutes = require('./Routes/playerRoutes');   // 플레이어 라우트 폴더 추가
const app = express();
const port = 4000;                                      // 포트 4000번대

app.use(express.json());                                // JSON 통신 설정
app.use('/api',playerRoutes);                           // API 라우트 설정
const resourceFilePath = 'resources.json'               // 자원 저장 파일 경로

loadResource();

function loadResource()
{
    if(fs.existsSync(resourceFilePath))                 // 파일경로를 확인해서 파일이 있는지 확인
    {
            global.players = JSON.parse(data);
    }
    else
    {
        global.players = {};                            // 초기화
    }
}

app.listen(port, () =>
{
    console.log(`서버가 http://localhost:${port}에서 실행 중 입니다.`)
})
