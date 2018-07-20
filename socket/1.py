import socket

address = ('127.0.0.3', 8000)
soc = socket.socket()
soc.bind(address)
soc.listen(3)
print('waiting...')
conn, ad = soc.accept()
while 1:
    data = conn.recv(1024)
    print('...', str(data, 'utf8'))
    if not data:
        break
    text = input('>>>')
    conn.send(bytes(text, 'utf8'))
soc.close()


