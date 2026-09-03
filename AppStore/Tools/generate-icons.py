from pathlib import Path
from math import hypot
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / "Assets"
RIBBON = ROOT / "Ribbon" / "LayerRenamer"
FONT_CANDIDATES = [
    Path("C:/Windows/Fonts/arialbd.ttf"),
    Path("/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf"),
]
FONT = next((path for path in FONT_CANDIDATES if path.exists()), None)


def render(size: int) -> Image.Image:
    scale = 4
    canvas = Image.new("RGBA", (size * scale, size * scale), (0, 0, 0, 0))
    draw = ImageDraw.Draw(canvas)

    def points(values):
        return [(round(x * size * scale / 1024), round(y * size * scale / 1024)) for x, y in values]

    def rounded_polygon(values, radius):
        sampled = []
        count = len(values)

        def toward(origin, target, distance):
            dx = target[0] - origin[0]
            dy = target[1] - origin[1]
            length = hypot(dx, dy)
            return origin[0] + dx * distance / length, origin[1] + dy * distance / length

        for index, corner in enumerate(values):
            previous = values[index - 1]
            following = values[(index + 1) % count]
            entry = toward(corner, previous, radius)
            exit_point = toward(corner, following, radius)
            sampled.append(entry)
            for step in range(1, 9):
                t = step / 8
                one_minus_t = 1 - t
                sampled.append((
                    one_minus_t * one_minus_t * entry[0]
                    + 2 * one_minus_t * t * corner[0]
                    + t * t * exit_point[0],
                    one_minus_t * one_minus_t * entry[1]
                    + 2 * one_minus_t * t * corner[1]
                    + t * t * exit_point[1],
                ))
        return points(sampled)

    layers = [
        ([(512, 330), (904, 562), (512, 794), (120, 562)], "#737373"),
        ([(512, 140), (904, 372), (512, 604), (120, 372)], "#4ba9e6"),
    ]
    for polygon, colour in layers:
        draw.polygon(rounded_polygon(polygon, 58), fill=colour)

    font_size = max(6, round(size * scale * 0.215))
    font = ImageFont.truetype(str(FONT), font_size) if FONT else ImageFont.load_default(font_size)
    draw.text(
        (size * scale / 2, size * scale * (372 / 1024)),
        "LR",
        font=font,
        fill="white",
        anchor="mm",
        stroke_width=0,
    )
    return canvas.resize((size, size), Image.Resampling.LANCZOS)


def render_ribbon(size: int) -> Image.Image:
    source = render(256)
    bounds = source.getchannel("A").getbbox()
    artwork = source.crop(bounds)
    maximum = size - 2
    ratio = min(maximum / artwork.width, maximum / artwork.height)
    width = max(1, round(artwork.width * ratio))
    height = max(1, round(artwork.height * ratio))
    artwork = artwork.resize((width, height), Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    canvas.alpha_composite(artwork, ((size - width) // 2, (size - height) // 2))
    return canvas


ASSETS.mkdir(parents=True, exist_ok=True)
RIBBON.mkdir(parents=True, exist_ok=True)

render(1024).save(ASSETS / "LayerRenamerIcon-1024.png")
render(120).save(ASSETS / "LayerRenamerIcon-120.png")
render(100).save(ASSETS / "LayerRenamerIcon-100.png")
render_ribbon(32).save(RIBBON / "LayerRenamer32.png")
render_ribbon(16).save(RIBBON / "LayerRenamer16.png")

icon = render(256)
icon.save(
    ASSETS / "LayerRenamerIcon.ico",
    format="ICO",
    sizes=[(16, 16), (24, 24), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)],
)
icon.save(
    ROOT.parent / "Layer Renamer App for AutoCAD" / "LayerRenamerIcon.ico",
    format="ICO",
    sizes=[(16, 16), (24, 24), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)],
)
