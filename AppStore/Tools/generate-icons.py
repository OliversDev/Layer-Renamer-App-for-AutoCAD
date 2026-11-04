from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / "Assets"
RIBBON = ROOT / "Ribbon" / "Renamer"
PROJECT = ROOT.parent / "Renamer App for AutoCAD"
FONT_CANDIDATES = [
    Path("C:/Windows/Fonts/arialbd.ttf"),
    Path("/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf"),
]
FONT = next((path for path in FONT_CANDIDATES if path.exists()), None)


def render(size: int) -> Image.Image:
    scale = 4
    pixels = size * scale
    image = Image.new("RGBA", (pixels, pixels), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)
    margin = round(pixels * 0.11)
    radius = round(pixels * 0.15)
    draw.rounded_rectangle(
        (margin, margin, pixels - margin, pixels - margin),
        radius=radius,
        fill="#4ba9e6",
    )

    font_size = max(8, round(pixels * 0.43))
    font = ImageFont.truetype(str(FONT), font_size) if FONT else ImageFont.load_default()
    draw.text((pixels / 2, pixels * 0.42), "R", font=font, fill="white", anchor="mm")

    width = max(3, round(pixels * 0.052))
    white = "#ffffff"
    grey = "#737373"
    draw.line((pixels * 0.24, pixels * 0.70, pixels * 0.65, pixels * 0.70), fill=white, width=width)
    draw.line((pixels * 0.65, pixels * 0.70, pixels * 0.58, pixels * 0.63), fill=white, width=width)
    draw.line((pixels * 0.65, pixels * 0.70, pixels * 0.58, pixels * 0.77), fill=white, width=width)
    draw.line((pixels * 0.76, pixels * 0.82, pixels * 0.35, pixels * 0.82), fill=grey, width=width)
    draw.line((pixels * 0.35, pixels * 0.82, pixels * 0.42, pixels * 0.75), fill=grey, width=width)
    draw.line((pixels * 0.35, pixels * 0.82, pixels * 0.42, pixels * 0.89), fill=grey, width=width)
    return image.resize((size, size), Image.Resampling.LANCZOS)


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
for size in (1024, 120, 100):
    render(size).save(ASSETS / f"RenamerIcon-{size}.png")
render_ribbon(32).save(RIBBON / "Renamer32.png")
render_ribbon(16).save(RIBBON / "Renamer16.png")

icon = render(256)
for target in (ASSETS / "RenamerIcon.ico", PROJECT / "RenamerIcon.ico"):
    icon.save(
        target,
        format="ICO",
        sizes=[(16, 16), (24, 24), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)],
    )
