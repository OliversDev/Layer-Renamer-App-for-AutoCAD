from pathlib import Path
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

    layers = [
        ([(512, 512), (900, 736), (512, 960), (124, 736)], "#3f3f3f"),
        ([(512, 302), (900, 526), (512, 750), (124, 526)], "#737373"),
        ([(512, 72), (900, 296), (512, 520), (124, 296)], "#4ba9e6"),
    ]
    for polygon, colour in layers:
        draw.polygon(points(polygon), fill=colour)

    font_size = max(6, round(size * scale * 0.205))
    font = ImageFont.truetype(str(FONT), font_size) if FONT else ImageFont.load_default(font_size)
    draw.text(
        (size * scale / 2, size * scale * 0.327),
        "LR",
        font=font,
        fill="white",
        anchor="mm",
        stroke_width=0,
    )
    return canvas.resize((size, size), Image.Resampling.LANCZOS)


ASSETS.mkdir(parents=True, exist_ok=True)
RIBBON.mkdir(parents=True, exist_ok=True)

render(1024).save(ASSETS / "LayerRenamerIcon-1024.png")
render(120).save(ASSETS / "LayerRenamerIcon-120.png")
render(100).save(ASSETS / "LayerRenamerIcon-100.png")
render(32).save(RIBBON / "LayerRenamer32.png")
render(16).save(RIBBON / "LayerRenamer16.png")

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
