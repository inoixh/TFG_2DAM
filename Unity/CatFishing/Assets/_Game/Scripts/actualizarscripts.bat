@echo off
:: Crea el archivo (o lo vacía si ya existe) y mete el contenido de todos los .cs
(for %%f in (*.cs) do (
    echo // ==========================================
    echo // ARCHIVO: %%f
    echo // ==========================================
    type "%%f"
    echo.
    echo.
)) > all_scripts.txt

echo Proceso terminado.
