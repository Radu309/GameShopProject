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

connection.on("ReceiveMessage", (sender, messageDto) => {
    console.log("Current Sender: ", senderId.value);
    console.log("Current Receiver: ", receiverId.value);
    console.log("Received message:", messageDto);
    if(messageDto.sender === senderId.value || messageDto.sender === receiverId.value) 
        appendMessageToList(sender, messageDto);
});

sendButton.addEventListener("click", event => {
    event.preventDefault();

    const sender = senderId.value;
    const receiver = receiverId.value;
    const message = messageToSend.value;

    if (receiver !== "" && message !== "" && sender !== "") {
        connection.invoke("SendMessageToOne", sender, receiver, message)
            .catch(err => {
                console.error("Error sending private message:", err.toString());
            });
    }
});

const encodeMessage = (user, message) => {
    const safeMessage = message
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;");
    return `${safeMessage}`;
};

const appendMessageToList = (user, messageDto) => {
    const encodedMsg = encodeMessage(user, messageDto.message);
    const messageDate = new Date(messageDto.timestamp); 
    const formattedTime = messageDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

    const messageDiv = document.createElement("div");
    if (messageDto.sender === senderId.value) {
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

