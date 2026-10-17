#!/usr/bin/env python3
"""Generates the launcher icons of the Android app.

The icon is a stylised plumbob on a dark rounded square. Run this script
whenever the icon should change:

    python3 Tools/generate_icons.py

It needs Pillow (pip install Pillow).
"""

import math
import os

from PIL import Image, ImageDraw

HERE = os.path.dirname(os.path.abspath(__file__))
RESOURCES = os.path.join(os.path.dirname(HERE), "Resources")

DENSITIES = {
    "mdpi": 48,
    "hdpi": 72,
    "xhdpi": 96,
    "xxhdpi": 144,
    "xxxhdpi": 192,
}

BACKGROUND = (24, 30, 40, 255)
BACKGROUND_EDGE = (44, 56, 74, 255)
PLUMB_TOP = (196, 255, 120, 255)
PLUMB_BOTTOM = (44, 150, 70, 255)
PLUMB_EDGE = (232, 255, 214, 255)


def draw_plumbob(draw, cx, cy, height, width):
    """Draws a plumbob (an elongated octahedron) centred on (cx, cy)."""
    top = (cx, cy - height / 2)
    right = (cx + width / 2, cy)
    bottom = (cx, cy + height / 2)
    left = (cx - width / 2, cy)
    middle = (cx, cy)

    # The left half is darker, the right half is lighter.
    draw.polygon([top, right, bottom], fill=PLUMB_BOTTOM)
    draw.polygon([top, left, bottom], fill=PLUMB_TOP)
    draw.polygon([top, middle, right], fill=(150, 230, 110, 255))
    draw.polygon([middle, right, bottom], fill=(30, 120, 60, 255))

    # Outline.
    draw.line([top, right, bottom, left, top], fill=PLUMB_EDGE, width=max(1, int(height / 28)))


def make_icon(size, round_icon=False):
    scale = 4
    big = size * scale
    image = Image.new("RGBA", (big, big), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)

    if round_icon:
        draw.ellipse((0, 0, big - 1, big - 1), fill=BACKGROUND, outline=BACKGROUND_EDGE,
                     width=max(1, int(big / 24)))
    else:
        radius = int(big * 0.22)
        draw.rounded_rectangle((0, 0, big - 1, big - 1), radius=radius, fill=BACKGROUND,
                               outline=BACKGROUND_EDGE, width=max(1, int(big / 24)))

    draw_plumbob(draw, big / 2, big * 0.47, big * 0.52, big * 0.34)

    return image.resize((size, size), Image.LANCZOS)


def main():
    for density, size in DENSITIES.items():
        folder = os.path.join(RESOURCES, "mipmap-" + density)
        os.makedirs(folder, exist_ok=True)
        make_icon(size).save(os.path.join(folder, "ic_launcher.png"))
        make_icon(size, round_icon=True).save(os.path.join(folder, "ic_launcher_round.png"))
        print("wrote", folder)


if __name__ == "__main__":
    main()
