import * as signalR from '@microsoft/signalr'

export function createHub() {
  return new signalR.HubConnectionBuilder()
    .withUrl(import.meta.env.VITE_HUB_URL)
    .withAutomaticReconnect()
    .build()
}