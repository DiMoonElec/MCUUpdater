import re
import os
import shutil
import subprocess
import sys
from collections import namedtuple
from typing import NamedTuple

# Определяем структуру для версии
class Version(NamedTuple):
    major: int
    minor: int
    patch: int
    build: int

    def __str__(self):
        return f"{self.major}.{self.minor}.{self.patch}.{self.build}"

def read_version(assembly_info_path: str) -> Version:
    """
    Читает AssemblyFileVersion и AssemblyVersion из файла AssemblyInfo.cs.
    Возвращает Version. Кидает исключение при ошибке.
    """
    if not os.path.exists(assembly_info_path):
        raise FileNotFoundError(f"Файл не найден: {assembly_info_path}")
    
    with open(assembly_info_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Парсим версию с помощью regex
    match_file = re.search(r'AssemblyFileVersion\("(\d+)\.(\d+)\.(\d+)\.(\d+)"\)', content)
    match_assembly = re.search(r'AssemblyVersion\("(\d+)\.(\d+)\.(\d+)\.(\d+)"\)', content)
    
    if not match_file or not match_assembly:
        raise ValueError("Не удалось найти AssemblyFileVersion или AssemblyVersion в файле.")
    
    # Проверяем, что версии совпадают
    if match_file.groups() != match_assembly.groups():
        raise ValueError("AssemblyFileVersion и AssemblyVersion не совпадают в файле.")
    
    major, minor, patch, build = map(int, match_file.groups())
    return Version(major, minor, patch, build)

def write_version(assembly_info_path: str, version: Version) -> None:
    """
    Записывает новую версию в AssemblyFileVersion и AssemblyVersion.
    Кидает исключение при ошибке.
    """
    if not os.path.exists(assembly_info_path):
        raise FileNotFoundError(f"Файл не найден: {assembly_info_path}")
    
    with open(assembly_info_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Заменяем AssemblyVersion
    content = re.sub(
        r'AssemblyVersion\("\d+\.\d+\.\d+\.\d+"\)',
        f'AssemblyVersion("{version}")',
        content
    )
    
    # Заменяем AssemblyFileVersion
    content = re.sub(
        r'AssemblyFileVersion\("\d+\.\d+\.\d+\.\d+"\)',
        f'AssemblyFileVersion("{version}")',
        content
    )
    
    with open(assembly_info_path, 'w', encoding='utf-8') as f:
        f.write(content)

def increment_version(current_version: Version) -> Version:
    """
    Запрашивает у пользователя тип инкремента (major, minor, patch, none).
    Инкрементит соответствующую часть, сбрасывая младшие (если не none).
    Всегда инкрементит build на 1.
    Возвращает новую Version. Кидает исключение при неверном вводе.
    """
    increment_type = input("Введите тип инкремента (major/minor/patch/none): ").strip().lower()
    
    major, minor, patch, build = current_version
    
    if increment_type == 'major':
        major += 1
        minor = 0
        patch = 0
        build += 1
    elif increment_type == 'minor':
        minor += 1
        patch = 0
        build += 1
    elif increment_type == 'patch':
        patch += 1
        build += 1
    elif increment_type == 'none':
        pass
    else:
        raise ValueError("Неверный тип инкремента. Допустимо: major, minor, patch, none.")
    
    return Version(major, minor, patch, build)

def find_msbuild() -> str:
    """
    Автоматически ищет путь к MSBuild.exe.
    Проверяет типичные локации Visual Studio.
    Кидает исключение, если не найден.
    """
    possible_paths = [
        r"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe",
        r"C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
        r"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe",
        r"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
        # Добавьте больше, если нужно
    ]
    
    for path in possible_paths:
        if os.path.exists(path):
            return path
    
    # Альтернатива: Использовать vswhere.exe, если установлен
    vswhere_path = r"C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"
    if os.path.exists(vswhere_path):
        try:
            output = subprocess.check_output([
                vswhere_path, "-latest", "-products", "*", "-requires", "Microsoft.Component.MSBuild",
                "-property", "installationPath"
            ]).decode().strip()
            if output:
                msbuild_path = os.path.join(output, r"MSBuild\Current\Bin\MSBuild.exe")
                if os.path.exists(msbuild_path):
                    return msbuild_path
        except subprocess.CalledProcessError:
            pass
    
    raise FileNotFoundError("MSBuild.exe не найден. Установите Visual Studio или укажите путь вручную.")

def build_solution(solution_path: str, configuration: str = 'Release') -> None:
    """
    Собирает решение с помощью MSBuild.
    Кидает исключение при ошибке сборки.
    """
    msbuild_path = find_msbuild()
    if not os.path.exists(solution_path):
        raise FileNotFoundError(f"Файл решения не найден: {solution_path}")
    
    try:
        subprocess.check_call([
            msbuild_path,
            solution_path,
            f"/p:Configuration={configuration}",
            "/t:Rebuild",
            "/verbosity:minimal"
        ])
    except subprocess.CalledProcessError as e:
        raise RuntimeError(f"Ошибка сборки решения: {e}")

def merge_release_folders(sources: list[str], target: str) -> None:
    """
    Копирует содержимое из нескольких исходных папок в одну целевую папку.
    Перед копированием очищает содержимое целевой папки, если она существует.
    Файлы с одинаковыми именами перезаписываются (последние источники имеют приоритет).
    Кидает исключение при ошибке.
    """
    if not sources:
        raise ValueError("Список источников не может быть пустым.")
    
    # Создаем целевую папку, если не существует
    os.makedirs(target, exist_ok=True)
    
    # Очищаем содержимое целевой папки, если она не пустая
    try:
        for item in os.listdir(target):
            item_path = os.path.join(target, item)
            if os.path.isdir(item_path):
                shutil.rmtree(item_path)
            else:
                os.unlink(item_path)
    except Exception as e:
        raise RuntimeError(f"Ошибка очистки целевой папки {target}: {e}")
    
    for source in sources:
        if not os.path.exists(source):
            raise FileNotFoundError(f"Исходная папка не найдена: {source}")
        
        if not os.path.isdir(source):
            raise ValueError(f"Источник должен быть папкой: {source}")
        
        try:
            # Копируем дерево, перезаписывая существующие файлы (dirs_exist_ok=True требует Python 3.8+)
            shutil.copytree(source, target, dirs_exist_ok=True)
        except shutil.Error as e:
            raise RuntimeError(f"Ошибка копирования из {source}: {e}")
        except Exception as e:
            raise RuntimeError(f"Неизвестная ошибка при копировании из {source}: {e}")

def find_iscc() -> str:
    """
    Автоматически ищет путь к ISCC.exe (Inno Setup Compiler).
    Кидает исключение, если не найден.
    """
    possible_paths = [
        r"C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
        r"C:\Program Files\Inno Setup 6\ISCC.exe",
        # Добавьте больше, если нужно
    ]
    
    for path in possible_paths:
        if os.path.exists(path):
            return path
    
    raise FileNotFoundError("ISCC.exe не найден. Установите Inno Setup или укажите путь вручную.")

def build_installer(iss_path: str) -> None:
    """
    Запускает сборку инсталлятора с помощью Inno Setup.
    Кидает исключение при ошибке.
    """
    iscc_path = find_iscc()
    if not os.path.exists(iss_path):
        raise FileNotFoundError(f"Файл .iss не найден: {iss_path}")
    
    try:
        subprocess.check_call([iscc_path, "/Q", iss_path])
    except subprocess.CalledProcessError as e:
        raise RuntimeError(f"Ошибка сборки инсталлятора: {e}")

def get_script_directory() -> str:
    """
    Возвращает абсолютный путь к директории, где находится текущий скрипт.
    Это полезно для вычисления относительных путей независимо от рабочей директории.
    Кидает исключение, если __file__ не определён (например, в REPL).
    """
    if '__file__' not in globals():
        raise RuntimeError("Эта функция должна вызываться из скрипта, а не из интерактивной оболочки.")
    
    # Получаем абсолютный путь к файлу скрипта
    script_path = os.path.abspath(__file__)
    
    # Извлекаем директорию
    script_dir = os.path.dirname(script_path)
    
    return script_dir


if __name__ == "__main__":
    try:
    # ==================== НАСТРОЙКИ ====================
        # путь к AssemblyInfo.cs GUI-утилиты
        assembly_info_path_gui = r"..\source\MCUUpdaterGUI\Properties\AssemblyInfo.cs" 
        
        # путь к AssemblyInfo.cs CLI-утилиты
        assembly_info_path_cli = r"..\source\MCUUpdater\Properties\AssemblyInfo.cs" 

        # путь к решению
        solution_path = r"..\source\MCUUpdater.sln"

        # путь к .iss файлу        
        iss_path = r"installer.iss"
        
        # Папки Release проектов
        release_sources = [
            r"..\source\MCUUpdaterGUI\bin\Release", # GUI утилита
            r"..\source\MCUUpdater\bin\Release", # CLI утилита
        ]

        # Общая папка, куда всё сольётся
        combined_release_path = r"ReleaseCombined"

        # =========================================================
        # 0. Готовим пути сборки
        script_dir = get_script_directory()

        abs_assembly_info_path_gui = os.path.join(script_dir, assembly_info_path_gui)
        abs_assembly_info_path_cli = os.path.join(script_dir, assembly_info_path_cli)
        abs_solution_path = os.path.join(script_dir, solution_path)

        abs_release_sources = []
        for s in release_sources:
            abs_release_sources.append(os.path.join(script_dir, s))

        abs_combined_release_path = os.path.join(script_dir, combined_release_path)

        abs_iss_path = os.path.join(script_dir, iss_path)

        # 1. Читаем текущую версию
        current_version = read_version(os.path.join(script_dir, assembly_info_path_gui))
        print(f"Текущая версия: {current_version}")
        
        # 2. Запрашиваем инкремент и получаем новую версию
        new_version = increment_version(current_version)
        print(f"Новая версия: {new_version}")

        # 3. Записываем новую версию в AssemblyInfo.cs
        write_version(abs_assembly_info_path_gui, new_version)
        write_version(abs_assembly_info_path_cli, new_version)

        # 4. Собираем решение
        print("Сборка решения...")
        build_solution(abs_solution_path)

        # 5. Сливаем все Release-папки в одну общую
        print(f"Слияние Release-папок в {combined_release_path}...")
        merge_release_folders(abs_release_sources, abs_combined_release_path)

        # 6. Собираем инсталлятор из общей папки
        # Важно: в .iss файле в секции [Files] должен быть указан Source: "ReleaseCombined\*"
        print("Сборка инсталлятора...")
        build_installer(abs_iss_path)

        print(f"\nГотово! Релиз версии {new_version} собран.")
        # =========================================================
    except Exception as e:
        print(f"\nERROR: {e}", file=sys.stderr)
        sys.exit(1)


