#!/usr/bin/env python3
"""
Generates the on-screen control icons of Simitone.

The icons are plain white line art on a transparent background - the touch
system tints them at runtime, so a single set works with every theme.

Run with:

    python3 generate_icons.py

Requires Pillow (``pip install Pillow``). The generated PNGs are checked into
the repository, so this script only has to be run when the icon design changes.
"""

import math
import os

from PIL import Image, ImageDraw

SIZE = 256
STROKE = 20
WHITE = (255, 255, 255, 255)
CLEAR = (0, 0, 0, 0)

OUT_DIR = os.path.dirname(os.path.abspath(__file__))


# --------------------------------------------------------------------------- #
# drawing helpers
# --------------------------------------------------------------------------- #
def new_image():
    img = Image.new("RGBA", (SIZE, SIZE), CLEAR)
    return img, ImageDraw.Draw(img)


def cap(draw, x, y, width):
    """Draws a round cap / joint at the given position."""
    r = width / 2.0
    draw.ellipse([x - r, y - r, x + r, y + r], fill=WHITE)


def polyline(draw, points, width=STROKE, closed=False):
    pts = list(points)
    if closed:
        pts = pts + [pts[0]]
    draw.line(pts, fill=WHITE, width=width, joint="curve")
    for (x, y) in pts:
        cap(draw, x, y, width)


def arrow(draw, tip, direction, size=46, width=STROKE):
    """Draws a solid triangular arrow head pointing in `direction`."""
    dx, dy = direction
    length = math.hypot(dx, dy)
    dx, dy = dx / length, dy / length
    # perpendicular
    px, py = -dy, dx
    base_x = tip[0] - dx * size
    base_y = tip[1] - dy * size
    p1 = (base_x + px * size * 0.62, base_y + py * size * 0.62)
    p2 = (base_x - px * size * 0.62, base_y - py * size * 0.62)
    draw.polygon([tip, p1, p2], fill=WHITE)


def arc_arrow(draw, center, radius, start_deg, end_deg, head_deg, width=STROKE,
              head_size=44, clockwise=True):
    """Draws a circular arrow (an arc with an arrow head at `head_deg`)."""
    box = [center[0] - radius, center[1] - radius,
           center[0] + radius, center[1] + radius]
    draw.arc(box, start=start_deg, end=end_deg, fill=WHITE, width=width)

    a = math.radians(head_deg)
    point = (center[0] + radius * math.cos(a), center[1] + radius * math.sin(a))
    if clockwise:
        tangent = (math.sin(a), -math.cos(a))
    else:
        tangent = (-math.sin(a), math.cos(a))
    arrow(draw, point, tangent, size=head_size, width=width)


def rounded_rect(draw, box, radius, width=STROKE, fill=None):
    if fill is not None:
        draw.rounded_rectangle(box, radius=radius, fill=fill)
    else:
        draw.rounded_rectangle(box, radius=radius, outline=WHITE, width=width)


def save(img, name):
    path = os.path.join(OUT_DIR, name + ".png")
    img.save(path, "PNG", optimize=True)
    print("wrote", os.path.relpath(path, OUT_DIR))


# --------------------------------------------------------------------------- #
# the individual icons
# --------------------------------------------------------------------------- #
def icon_arrow(name, direction):
    img, d = new_image()
    dx, dy = direction
    start = (128 - dx * 52, 128 - dy * 52)
    end = (128 + dx * 52, 128 + dy * 52)
    polyline(d, [start, end])
    arrow(d, end, direction)
    save(img, name)


def icon_zoom(name, plus):
    img, d = new_image()
    cx, cy, r = 112, 112, 56
    d.ellipse([cx - r, cy - r, cx + r, cy + r], outline=WHITE, width=STROKE)
    # handle
    polyline(d, [(cx + r * 0.72, cy + r * 0.72), (196, 196)])
    if plus:
        polyline(d, [(cx - 22, cy), (cx + 22, cy)], width=16)
        polyline(d, [(cx, cy - 22), (cx, cy + 22)], width=16)
    else:
        polyline(d, [(cx - 22, cy), (cx + 22, cy)], width=16)
    save(img, name)


def icon_rotate(name, clockwise):
    img, d = new_image()
    if clockwise:
        arc_arrow(d, (128, 128), 66, 120, 380, 380, clockwise=True)
    else:
        arc_arrow(d, (128, 128), 66, 60, 320, 60, clockwise=False)
    save(img, name)


def icon_pause():
    img, d = new_image()
    rounded_rect(d, [86, 60, 116, 196], 14, fill=WHITE)
    rounded_rect(d, [140, 60, 170, 196], 14, fill=WHITE)
    save(img, "pause")


def icon_play():
    img, d = new_image()
    d.polygon([(88, 56), (200, 128), (88, 200)], fill=WHITE)
    save(img, "play")


def icon_speed(count):
    img, d = new_image()
    for i in range(count):
        x = 128 + (i - (count - 1) / 2.0) * 46
        d.polygon([(x - 20, 74), (x + 20, 128), (x - 20, 182)], fill=WHITE)
    save(img, "speed_%d" % count)


def icon_frame_advance():
    img, d = new_image()
    d.polygon([(72, 62), (168, 128), (72, 194)], fill=WHITE)
    rounded_rect(d, [182, 62, 206, 194], 12, fill=WHITE)
    save(img, "frame_advance")


def icon_person(d, cx, cy, scale=1.0):
    """A simple head + shoulders silhouette."""
    r = 30 * scale
    d.ellipse([cx - r, cy - r * 1.25, cx + r, cy + r * 0.75], fill=WHITE)
    body_w = 62 * scale
    body_top = cy + r * 0.9
    d.rounded_rectangle([cx - body_w / 2, body_top, cx + body_w / 2, cy + 118 * scale],
                        radius=int(28 * scale), fill=WHITE)


def icon_mode_live():
    img, d = new_image()
    icon_person(d, 128, 84, 1.0)
    save(img, "mode_live")


def icon_mode_build():
    img, d = new_image()
    # hammer: handle + head
    polyline(d, [(78, 186), (150, 92)], width=22)
    polyline(d, [(126, 60), (186, 120)], width=34)
    polyline(d, [(120, 66), (146, 40)], width=26)
    save(img, "mode_build")


def icon_mode_buy():
    img, d = new_image()
    # shopping bag: a wide body with two handle arcs on top
    d.rounded_rectangle([48, 108, 208, 208], radius=16, outline=WHITE, width=STROKE)
    d.arc([84, 52, 128, 108], start=180, end=360, fill=WHITE, width=STROKE)
    d.arc([128, 52, 172, 108], start=180, end=360, fill=WHITE, width=STROKE)
    save(img, "mode_buy")


def icon_options():
    img, d = new_image()
    cx = cy = 128
    outer, inner = 84, 52
    teeth = 8
    for i in range(teeth):
        a = math.radians(i * 360.0 / teeth)
        x1 = cx + math.cos(a) * (inner - 6)
        y1 = cy + math.sin(a) * (inner - 6)
        x2 = cx + math.cos(a) * (outer + 6)
        y2 = cy + math.sin(a) * (outer + 6)
        polyline(d, [(x1, y1), (x2, y2)], width=26)
    d.ellipse([cx - outer, cy - outer, cx + outer, cy + outer], outline=WHITE, width=STROKE)
    d.ellipse([cx - inner, cy - inner, cx + inner, cy + inner], outline=WHITE, width=STROKE)
    save(img, "options")


def icon_inventory():
    img, d = new_image()
    # a crate / box
    d.rounded_rectangle([56, 84, 200, 206], radius=16, outline=WHITE, width=STROKE)
    d.line([(56, 122), (200, 122)], fill=WHITE, width=STROKE)
    d.line([(128, 84), (128, 206)], fill=WHITE, width=STROKE)
    save(img, "inventory")


def icon_family():
    img, d = new_image()
    icon_person(d, 92, 78, 0.72)
    icon_person(d, 172, 92, 0.62)
    save(img, "family")


def icon_skills():
    img, d = new_image()
    # ascending bars + arrow
    polyline(d, [(64, 186), (64, 146)])
    polyline(d, [(108, 186), (108, 112)])
    polyline(d, [(152, 186), (152, 88)])
    polyline(d, [(78, 96), (168, 52)], width=16)
    arrow(d, (178, 46), (1.0, -0.5), size=34, width=16)
    save(img, "skills")


def icon_cancel():
    img, d = new_image()
    polyline(d, [(80, 80), (176, 176)])
    polyline(d, [(176, 80), (80, 176)])
    save(img, "cancel")


def icon_menu():
    img, d = new_image()
    for y in (78, 128, 178):
        polyline(d, [(66, y), (190, y)])
    save(img, "menu")


def icon_edit():
    img, d = new_image()
    # sliders
    polyline(d, [(56, 84), (200, 84)])
    polyline(d, [(56, 172), (200, 172)])
    d.ellipse([96, 62, 140, 106], fill=WHITE)
    d.ellipse([140, 150, 184, 194], fill=WHITE)
    save(img, "edit")


def icon_free_camera():
    img, d = new_image()
    # an eye
    d.ellipse([52, 88, 204, 168], outline=WHITE, width=STROKE)
    d.ellipse([104, 104, 152, 152], fill=WHITE)
    save(img, "free_camera")


def icon_unknown():
    img, d = new_image()
    d.ellipse([56, 56, 200, 200], outline=WHITE, width=STROKE)
    polyline(d, [(104, 96), (128, 120), (128, 140)], width=18)
    d.ellipse([120, 152, 136, 168], fill=WHITE)
    save(img, "unknown")


def main():
    icon_arrow("camera_forward", (0, -1))
    icon_arrow("camera_back", (0, 1))
    icon_arrow("camera_left", (-1, 0))
    icon_arrow("camera_right", (1, 0))

    icon_zoom("zoom_in", True)
    icon_zoom("zoom_out", False)

    icon_rotate("rotate_left", False)
    icon_rotate("rotate_right", True)

    icon_free_camera()

    icon_pause()
    icon_play()
    icon_speed(1)
    icon_speed(2)
    icon_speed(3)
    icon_frame_advance()

    icon_mode_live()
    icon_mode_build()
    icon_mode_buy()

    icon_options()
    icon_inventory()
    icon_family()
    icon_skills()
    icon_cancel()
    icon_menu()
    icon_edit()
    icon_unknown()


if __name__ == "__main__":
    main()
