import requests
import re
import hmac
import time
import hashlib
import matplotlib.pyplot as plt
from PIL import Image
import base64
import json
from http import cookiejar

login_url = 'https://www.zhihu.com/signup'
login_api = 'https://www.zhihu.com/api/v3/oauth/sign_in'
form_data = {
    'client_id': 'c3cef7c66a1843f8b3a9e6a1e3160e20',
    'grant_type': 'password',
    'source': 'com.zhihu.web',
    'username': '',
    'password': '',
    'lang': 'en',
    'ref_source': 'homepage'
}

Headers = {
    'Connection': 'keep-alive',
    'Host': 'www.zhihu.com',
    'Referer': 'https://www.zhihu.com/',
    'User-Agent': 'Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)'
                  ' Chrome/64.0.3282.186 Safari/537.36'
}


class Zhihulogin(object):
    def __init__(self):
        self.login_url = login_url
        self.login_api = login_api
        self.session = requests.session()
        self.form_data = form_data.copy()
        self.session.headers = Headers.copy()
        self.session.cookies = cookiejar.LWPCookieJar(filename='./my_cookies.txt')

    def login(self, username=None, password=None, load_cookies=True):
        if load_cookies and self.load_cookies():
            if self.check_login():
                return True
        headers = self.session.headers.copy()
        headers.update({
            'authorization': 'oauth c3cef7c66a1843f8b3a9e6a1e3160e20',
            'X-Xsrftoken': self.get_token()
        })
        username, password = self.check_account_input(username, password)
        self.form_data.update({
            'username': username,
            'password': password
        })
        timestamp = str(int(time.time()) * 1000)
        self.form_data.update({
            'captcha': self.show_captcha(self.form_data['lang'], headers=headers),
            'timestamp': timestamp,
            'signature': self.get_signature(timestamp)
        })
        resp = self.session.post(self.login_api, data=self.form_data, headers=headers)
        if 'error' in resp.text:
            print(json.loads(resp.text)['error']['message'])
        elif self.check_login():
            return True
        print('Login fail!')
        return False

    def load_cookies(self):
        try:
            self.session.cookies.load(ignore_discard=True)
            return True
        except FileNotFoundError:
            return False

    def check_login(self):
        resp = self.session.get(self.login_url, allow_redirects=False)
        if resp.status_code == 302:
            self.session.cookies.save()
            print('Login success and manage to saving the cookies!')
            return True
        return False

    def show_captcha(self, lang, headers):
        if lang == 'cn':
            api = 'https://www.zhihu.com/api/v3/oauth/captcha?lang=cn'
        else:
            api = 'https://www.zhihu.com/api/v3/oauth/captcha?lang=en'
        resp = self.session.get(api, headers=headers)
        capt_show = re.search('true', resp.text)
        if capt_show:
            resp2cap = self.session.put(api, headers=headers)
            resp_dict = json.loads(resp2cap.content)
            img_base64 = resp_dict['img_base64'].replace('\n', ' ')
            with open('./captcha.jpg', 'wb') as f:
                f.write(base64.b64decode(img_base64))
            img = Image.open('./captcha.jpg')
            if lang == 'cn':
                plt.imshow(img)
                print('Please click the upside-down pictures')
                points = plt.ginput(7)
                capt = json.dumps({'img_size': [200, 44],
                                   'input_points': [[i[0] / 2, i[1] / 2] for i in points]})
            else:
                plt.imshow(img)
                cap = input('Please input the words:')
                self.session.post(api, data={'input_text': cap}, headers=headers)
                return cap
            return ''

    def get_token(self):
        resp = self.session.get(self.login_url)
        token = resp.cookies['_xsrf']
        return token

    def get_signature(self, timestamp):
        ha = hmac.new(b'd1b964811afb40118a12068ff74a12f4', digestmod=hashlib.sha1)
        grant_type = self.form_data['grant_type']
        client_id = self.form_data['client_id']
        source = self.form_data['source']
        ha.update(bytes((grant_type + client_id + source + timestamp), 'utf-8'))
        return ha.hexdigest()

    def check_account_input(self, username, password):
        if username is None:
            username = self.form_data['username']
            if not username:
                username = input('Please input the phone number:')
                if '+86' not in username:
                    username = '+86' + username
        if password is None:
            password = self.form_data['password']
            if not password:
                password = input('Please input the password:')
        return username, password


if __name__ == '__main__':
    log = Zhihulogin()
    log.login(username=None, password=None, load_cookies=True)
