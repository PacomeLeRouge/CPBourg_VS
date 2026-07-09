# Using the real CPBourg logo

This app currently shows a placeholder two-square mark (grey + orange)
wherever the logo appears (splash screen and the app header).

To use the real CPBourg logo instead:

1. Get the official logo file from CPBourg marketing, or from the
   official site's asset library:
   https://www.cpbourg.com/files/Library/CPBOURG-logo.pdf
   (convert to PNG if you're given the PDF/vector version - any image
   editor or an online PDF-to-PNG converter will do this in one step).
2. Save it as a transparent-background PNG named exactly:
       cpbourg-logo.png
3. Place it in this folder: `Assets/cpbourg-logo.png`
   (same folder as this file).
4. Rebuild the project.

That's it - no code changes needed. `LogoLoader.cs` checks for this file
at startup and automatically swaps it in for the placeholder mark on both
the splash screen and the app header. If the file isn't there, the app
falls back to the placeholder cleanly (it won't crash or show a broken
image icon).

**Recommended image specs:** roughly square or a horizontal lockup, at
least 150px tall, transparent background so it sits cleanly on both the
light splash/header background.

**Why this file exists:** the actual logo image couldn't be downloaded
directly into this project - the development sandbox this was built in
only has network access to a small allowlist of developer package
registries (npm, PyPI, GitHub, etc.), not general websites like
cpbourg.com. So this step has to happen on your machine.
