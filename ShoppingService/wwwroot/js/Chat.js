"use strict";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

const sendButton = document.getElementById("sendButton");
const senderId = document.getElementById("senderId");
const receiverId = document.getElementById("receiverId");
const messageToSend = document.getElementById("messageToSend");
const messagesList = document.getElementById("messagesList");

sendButton.disabled = true;

connection.start()
    .then(() => {
        sendButton.disabled = false;
    })
    .catch(err => {
        console.error("Connection error:", err.toString());
    });

connection.on("ReceiveMessage", (messageDto) => {
    if(messageDto.senderId === senderId.value || messageDto.senderId === receiverId.value) 
        appendMessageToList(messageDto);
});

sendButton.addEventListener("click", async event => {
    event.preventDefault();

    const sender = senderId.value;
    const receiver = receiverId.value;
    const message = messageToSend.value;

    if (receiver !== "" && message !== "" && sender !== "") {
        const requestData = { SenderId: sender, ReceiverId: receiver, Content: message };
        try {
            const response = await fetch("https://localhost:7078/api/grpc/send-message", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(requestData),
            });

            if (!response.ok) {
                console.error("Error sending message:", response.status, response.statusText);
                return;
            }
            const text = await response.text();
            const data = text ? JSON.parse(text) : {};
            connection.invoke("SendMessageToOne", sender, receiver, message)
                .catch(err => console.error("Error notifying chat:", err.toString()));
        } catch (err) {
            console.error("Error sending private message:", err.toString());
        }
    }
});

const encodeMessage = (message) => {
    const safeMessage = message
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;");
    return `${safeMessage}`;
};

const appendMessageToList = (messageDto) => {
    const encodedMsg = encodeMessage(messageDto.message);
    const messageDate = new Date(messageDto.timestamp); 
    const formattedTime = messageDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

    const messageDiv = document.createElement("div");
    if (messageDto.senderId === senderId.value) {
        messageDiv.classList.add("message", "message-sent");
    } else {
        messageDiv.classList.add("message", "message-received");
    }
    const messageText = document.createElement("p");
    messageText.classList.add("message-text");
    messageText.textContent = encodedMsg;

    const messageTime = document.createElement("span");
    messageTime.classList.add("message-time");
    messageTime.textContent = formattedTime;

    messageDiv.appendChild(messageText);
    messageDiv.appendChild(messageTime);
    messagesList.appendChild(messageDiv);
};

