import socket

address = ('127.0.0.3', 8000)
soc = socket.socket()
soc.connect(address)

while True:
    text = input('>>>')
    if text == 'exit1':
        break
    soc.send(bytes(text, 'utf8'))
    data = soc.recv(1024)
    print(str(data, 'utf8'))
