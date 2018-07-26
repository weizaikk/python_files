from PIL import Image, ImageDraw, ImageFont


def add_number(im):
    image = ImageDraw.Draw(im)
    font = ImageFont.truetype("C:/Windows/Fonts/Arial.ttf", size=20)
    fillcolor = "#ff0000"
    width, height = im.size
    image.text((width - 400, 100), "5", fillcolor, font)
    im.save('test.jpg', 'jpeg')
    return 0


im = Image.open('Lighthouse.jpg')
print(im.size, im.format, im.mode)
add_number(im)
