import { defineStore } from 'pinia'
import * as signalR from '@microsoft/signalr'
import http from '@/plugins/http'

//客服聊天室

export type MessageDto = {
    id: string
    ticketId: string
    senderRole: 'user' | 'agent'
    senderName: string
    content: string
    createdAt: string
}

export const useChatStore = defineStore('chat', {
    state: () => ({
        conn: null as signalR.HubConnection | null,
        // ticketId -> messages
        messages: {} as Record<string, MessageDto[]>,
        // 連線狀態
        status: 'disconnected' as 'disconnected' | 'connecting' | 'connected' | 'reconnecting',
    }),

    actions: {
        async ensureConnected() {
            if (this.conn && this.conn.state === signalR.HubConnectionState.Connected) return
            if (this.status === 'connecting' || this.status === 'reconnecting') return

            this.status = 'connecting'
            const connection = new signalR.HubConnectionBuilder()
                .withUrl(import.meta.env.VITE_HUB_URL)
                .withAutomaticReconnect()
                .build()

            // 訊息廣播：依 ticketId 分流
            connection.on('ReceiveMessage', (m: MessageDto) => {
                (this.messages[m.ticketId] ||= []).push(m)
            })

            // 連線狀態監聽
            connection.onreconnecting(() => { this.status = 'reconnecting' })
            connection.onreconnected(() => { this.status = 'connected' })
            connection.onclose(() => { this.status = 'disconnected' })

            await connection.start()
            this.conn = connection
            this.status = 'connected'
        },

        // 載入歷史 + 加入房間
        async join(ticketId: string) {
            await this.ensureConnected()
            if (!this.messages[ticketId]) {
                const { data } = await http.get<MessageDto[]>(`/api/messages/${ticketId}`)
                this.messages[ticketId] = data ?? []
            }
            await this.conn!.invoke('JoinRoom', ticketId)
        },

        async leave(ticketId: string) {
            if (this.conn) await this.conn.invoke('LeaveRoom', ticketId)
            // 不刪 messages，保留在 UI（需要時可清）
        },

        async send(ticketId: string, role: 'user' | 'agent', name: string, text: string) {
            await this.ensureConnected()
            await this.conn!.invoke('SendMessage', ticketId, role, name, text)
            // 不需手動 push，Hub 會廣播回來
        },

        // 可選：清除某房的暫存訊息
        clearRoom(ticketId: string) {
            delete this.messages[ticketId]
        }
    },
})
