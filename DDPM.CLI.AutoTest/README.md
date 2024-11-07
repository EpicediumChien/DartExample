# DDPM CLI AutoTest

- ## Setup (Python version must be between 3.13 and 3.14)
    - Install Poetry

        `pip install poetry`

    - Installing dependencies

        `poetry install`

- ## Build (Need to activate the virtual environment first)
    ```sh
    poetry shell # Activating the virtual environment
    pyinstaller -F .\ddpm_cli_auto_test.py
    ```

- ## Command line usage through python (python needs to be installed)
    - Activating the virtual environment

        `poetry shell`

    - All category in config.json

        `python .\ddpm_cli_auto_test.py`

    - Specify categories in config.json (Use commas to separate multiple categories)

        ```
        python .\ddpm_cli_auto_test.py --category Display
        python .\ddpm_cli_auto_test.py --category App
        python .\ddpm_cli_auto_test.py --category Peripherals
        python .\ddpm_cli_auto_test.py --category App,Peripherals
        ...
        ```

    - Show help message

        `python .\ddpm_cli_auto_test.py -h`

- ## Command line usage through executable file (no need to install python)
    - All category in config.json

        `.\ddpm_cli_auto_test.exe`

    - Specify directory in config.json (Use commas to separate multiple categories)

        ```
        .\ddpm_cli_auto_test.exe --category Display
        .\ddpm_cli_auto_test.exe --category App
        .\ddpm_cli_auto_test.exe --category Peripherals
        .\ddpm_cli_auto_test.exe --category App,Peripherals
        ...
        ```

    - Show help message

        `.\ddpm_cli_auto_test.exe -h`