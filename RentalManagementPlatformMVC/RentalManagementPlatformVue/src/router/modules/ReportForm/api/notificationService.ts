import * as signalR from "@microsoft/signalr";

// 建立一個 HubConnection 物件，並指後端的 Hub 路由
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub") // 必須與後端 app.MapHub 中設定的一致
    .withAutomaticReconnect() // 自動重連功能
    .build();

export function registerWarningHandler(handler: (message: string) => void) {
    // 建立監聽器"ReceiveWarning"
    connection.on("ReceiveWarning", (message) => {
        handler(message);//執行委派
    });
}

// 啟動連線，並在連線成功後訂閱指定的使用者群組
export async function startConnection(userId: number) {
    try {
        if (connection.state === signalR.HubConnectionState.Disconnected) {
            await connection.start();
            console.log("SignalR Connected.");

            // 連線成功後，呼叫後端 Hub 上的 JoinUserGroup 方法
            await connection.invoke("JoinUserGroup", userId.toString());
            console.log(`Joined user group: user_${userId}`);
        }
    } catch (err) {
        console.error("SignalR Connection Error: ", err);
        // 延遲 5 秒後重試
        setTimeout(() => startConnection(userId), 5000);
    }
}