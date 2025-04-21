
    Notification.requestPermission().then(permission => {
        if (permission === 'granted') {
            console.log('Notification permission granted.');
        } else {
            console.log('Notification permission denied.');
        }
    });

// app.js
const socket = new WebSocket('wss://localhost:7159'); 

const resultsList = document.getElementById('results');
const messageInput = document.getElementById('messageInput');
const sendButton = document.getElementById('sendButton');
const usernameInput = prompt("Enter your username:"); // Solicitar el nombre de usuario

// Evento cuando se abre la conexión
socket.addEventListener('open', () => {
    console.log('WebSocket connection established.');
    socket.send(JSON.stringify({ type: 'register', username: usernameInput }));
});
socket.addEventListener('message', (event) => {
    const data = JSON.parse(event.data); // Parsear el mensaje recibido
    if (data.type === 'message') {
        const message = data.message;
        console.log('Message received:', message);

        // Crear un nuevo elemento de lista para mostrar el mensaje
        const listItem = document.createElement('li');
        listItem.textContent = `${data.from}: ${message}`;
        resultsList.appendChild(listItem); // Agregar el mensaje a la lista

        // Mostrar una notificación push
        if (Notification.permission === 'granted') {
            new Notification('New Message', {
                body: `${data.from}: ${message}`,
                icon: '🔥'
            });
        }
    }
});
// Método para enviar mensajes al servidor
sendButton.addEventListener('click', () => {
    const message = messageInput.value.trim(); // Obtener el texto del campo de entrada
    const recipient = prompt("Mensaje A:"); 
    if (message && recipient) {
        // Enviar el mensaje al servidor con el destinatario
        socket.send(JSON.stringify({ type: 'message', to: recipient, message }));
        console.log('Message sent:', message);
        messageInput.value = ''; // Limpiar el campo de entrada
    } else if (message) {
        socket.send(message);
    }
    messageInput.value = '';
});