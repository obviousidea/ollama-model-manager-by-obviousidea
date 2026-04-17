from pathlib import Path

from PIL import Image, ImageDraw


def rounded_rectangle(draw, box, radius, fill):
    draw.rounded_rectangle(box, radius=radius, fill=fill)


def draw_icon(size: int) -> Image.Image:
    image = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)

    accent = (15, 118, 110, 255)
    accent_mid = (15, 118, 110, 215)
    accent_light = (15, 118, 110, 175)
    panel = (230, 255, 251, 255)
    white = (255, 255, 255, 255)
    eye_pupil = (15, 118, 110, 255)

    pad = int(size * 0.08)
    radius = int(size * 0.22)
    rounded_rectangle(draw, (pad, pad, size - pad, size - pad), radius, panel)

    left = int(size * 0.24)
    width = int(size * 0.46)
    height = int(size * 0.14)
    gap = int(size * 0.06)
    base_y = int(size * 0.26)
    layer_radius = int(size * 0.05)

    rounded_rectangle(draw, (left + int(size * 0.08), base_y, left + width + int(size * 0.08), base_y + height), layer_radius, accent)
    rounded_rectangle(draw, (left, base_y + height + gap, left + width, base_y + (2 * height) + gap), layer_radius, accent_mid)
    rounded_rectangle(draw, (left + int(size * 0.08), base_y + (2 * (height + gap)), left + width + int(size * 0.08), base_y + (3 * height) + (2 * gap)), layer_radius, accent_light)

    eye_box = (
        int(size * 0.57),
        int(size * 0.29),
        int(size * 0.87),
        int(size * 0.59),
    )
    draw.ellipse(eye_box, fill=white)

    iris_pad_x = int((eye_box[2] - eye_box[0]) * 0.28)
    iris_pad_y = int((eye_box[3] - eye_box[1]) * 0.24)
    draw.ellipse(
        (
            eye_box[0] + iris_pad_x,
            eye_box[1] + iris_pad_y,
            eye_box[2] - iris_pad_x,
            eye_box[3] - iris_pad_y,
        ),
        fill=eye_pupil,
    )

    highlight = (
        int(size * 0.19),
        int(size * 0.15),
        int(size * 0.41),
        int(size * 0.31),
    )
    draw.ellipse(highlight, fill=(255, 255, 255, 28))
    return image


def main() -> None:
    root = Path(__file__).resolve().parent.parent
    assets = root / "Assets"
    assets.mkdir(exist_ok=True)

    png_path = assets / "omm-stack-eye.png"
    ico_path = assets / "omm-stack-eye.ico"

    png = draw_icon(512)
    png.save(png_path)

    sizes = [(16, 16), (24, 24), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)]
    png.save(ico_path, format="ICO", sizes=sizes)


if __name__ == "__main__":
    main()
