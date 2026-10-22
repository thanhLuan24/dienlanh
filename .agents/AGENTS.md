# Design System & UI/UX Guidelines: CoolConnect Premium Tech Light Aesthetic

Whenever creating or modifying web interfaces/views in this project (CoolConnect), ALWAYS strictly adhere to the following design system and aesthetic rules.

## 1. Aesthetic Core Principles
- **Style Concept**: Premium minimalist, technical, clean, high-contrast light mode.
- **Tone**: Professional, high-performance, trusted, crisp marketplace for air conditioning & home appliance services.

## 2. Color Palette & Usage Standards

| Color Variable / Hex | Name | Primary Usage |
| --- | --- | --- |
| `--white`: `#FFFFFF` | Pure White | Main page background, card container backgrounds, navbar background. |
| `--charcoal`: `#F5F6F8` | Light Grey / Ice | Section backgrounds, sidebars, dashboard panels, muted overlays. |
| `--black`: `#111111` | Deep Charcoal | Primary headings, body text, high contrast titles, clean dark elements. |
| `--neon`: `#0D9488` | Cyber Teal / Ice Blue | Accent highlight color, active states, key KPIs/metrics, buttons background, link colors, AI Chatbot theme. |
| `--accent-gold`: `#FFC107` | Technical Gold | Ratings stars, featured highlights. |
| `--red`: `#E11D48` | Rose Red | Critical badges ("HOT", discount tags), urgent alerts, danger actions. |
| `--grey`: `#4B5563` | Slate Grey | Sub-descriptions, primary body copy, paragraph text. |
| `--grey-dark`: `#E5E7EB` | Light Border Grey | Subdued borders, table lines, dividers, scrollbar tracks. |
| `--grey-mid`: `#9CA3AF` | Mid Grey | Meta info, secondary labels, timestamps, brand titles. |

## 3. Typography Rules
- **Primary Font**: `'Space Grotesk', sans-serif` (Display titles, headings, body text, buttons).
- **Secondary / Mono Font**: `'DM Mono', monospace` (Labels, badges, status counters, technical metadata, code/ID displays).
- **Headings Styling**: All caps (`text-transform: uppercase`), tight line-height (`0.92` to `1.0`), negative letter-spacing for large titles.
- **Section Labels (`.label-mono`)**: Uppercase DM Mono, tracking `0.15em`, Cyber Teal `#0D9488`.

## 4. UI Components & Layout Guidelines
- **Navbar**: Light glassmorphism (`rgba(255,255,255,0.85)` with `backdrop-filter: blur`), subtle border bottom, Cyber Teal active tab indicator.
- **Cards & Layouts**: Sharp grid layouts with 1px border grid dividers (`rgba(0,0,0,0.06)`). Pure white background for product cards, store cards, and statistics widgets, surrounded by crisp light grey backgrounds.
- **Buttons**:
  - Primary Action (`btn-neon`): `#0D9488` background, `#FFFFFF` text, hover lift & teal shadow (`0 0 24px rgba(13, 148, 136, 0.25)`).
  - Secondary Action (`btn-outline`): Transparent background, thin grey/teal border, turns `#0D9488` border/text on hover.
- **Micro-animations**: Smooth transitions (`0.25s cubic-bezier(0.4, 0, 0.2, 1)`), scroll-triggered reveal animations (`data-animate`), pulsing teal dots (`.dot`).
- **Icons**: Bootstrap Icons (`bi-*`).

## 5. Application Across Future Features
All future pages and components (Store Public Page, Product Detail, Order Details, Repair Management & Tracking, Technician Portal, Admin Dashboard, etc.) MUST import `wwwroot/css/site.css` tokens and strictly maintain this premium light technical theme.

