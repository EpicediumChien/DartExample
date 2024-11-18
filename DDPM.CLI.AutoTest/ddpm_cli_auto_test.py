import subprocess
import json
import re
import os
from datetime import datetime
import argparse
import sys
import logging
import time

import pandas as pd
from colorlog import ColoredFormatter

logger = logging.getLogger(__name__)

class CLIAutoTest():
    def __init__(self, config, categories):
        self.config = config
        self.categories = categories
        self.cli_path = self.config["cli_path"]
        self.time = datetime.now().strftime("%Y%m%d_%H%M%S")
        self.report_dir = "Reports"
        self.report_file_name = f"report_{self.time}.xlsx"
        self.dfs = []
        self.device_data_dir = "DellDeviceData"
        self.get_device_data_pass = False
        self.pass_results = ["pass", "success", "completed", "complete"]
        self.results = {}
        
    def run(self, command):
        try:
            process = subprocess.run(f"{self.cli_path} {command}", 
                                    stdout=subprocess.PIPE, 
                                    check=False, 
                                    encoding="big5")
        
            return process.stdout
        except:
            logger.error(f"Fail to exec {command}")
            return "{}"
        finally:
            time.sleep(self.config["delay"])

    def parse_output(self, output):
        model_match = re.findall(r'"Model":\s"([^"]+)', output)
        result_match = re.findall(r'"Result":\s"([^"]+)', output)

        model = ", ".join(model_match) if model_match else "N/A"
        result = ", ".join(result_match) if result_match else "N/A"

        return model, result
    
    def check_get_connect_devices(self):
        logger.info("Checking connected devices...")
        command = "/get -app=DeviceData"
        output = self.run(command)
        _, result = self.parse_output(output)

        if not self.check_result_pass(result):
            logger.error("Device not connected.")
            return False

        return True
    
    def get_device_data(self, data: dict[str, list] = None):
        command = "/get -app=DeviceData"
        output = self.run(command)
        model, result = self.parse_output(output)

        if data:
            self.append_result(data, command, model, output, result)

        if self.check_result_pass(result):
            path = os.path.join(self.device_data_dir, f"device_data_{self.time}.json")
            with open(path, "w") as f:
                f.writelines(output)

            logger.info(f"Device data have been saved to '{path}'.")

            self.get_device_data_pass = True
            if not data:
                logger.info(f"Auto - Get device data result: '{result}'.")

        else:
            self.get_device_data_pass = False
            if not data:
                logger.warning(f"Auto - Get device data result: '{result}'.")

        return self.get_device_data_pass, result
    
    def set_device_data(self, command, data: dict[str, list]):
        output = self.run(command)
        model, result = self.parse_output(output)

        self.append_result(data, command, model, output, result)

        return result
    
    def check_result_pass(self, result: str):
        for pass_result in self.pass_results:
            if pass_result in result.lower():
                return True
            
        return False

    def main(self):
        if not os.path.exists(self.cli_path):
            logger.error(f"'{self.cli_path}' not exist.")
            sys.exit(1)

        os.makedirs(self.device_data_dir, exist_ok=True)
        os.makedirs(self.report_dir, exist_ok=True)

        if self.check_get_connect_devices():
            for category in self.categories:
                data = {
                    "Command": [],
                    "Model": [],
                    "Output": [],
                    "Result": [],
                }

                total_count = len(config["commands"][category])

                for index, command in enumerate(config["commands"][category]):
                    if "-app=devicedata" in command.lower():
                        _, result = self.get_device_data(data)
                        self.record_result(category, index, total_count, command, result)
                        continue

                    if "-app=deviceconfiguration" in command.lower():
                        if not self.get_device_data_pass and not self.get_device_data(None)[0]:
                            result = "Fail to get device data"
                        else:
                            result = self.set_device_data(command, data)

                        self.record_result(category, index, total_count, command, result)
                        continue

                    output = self.run(command)
                    model, result = self.parse_output(output)

                    self.append_result(data, command, model, output, result)
                    self.record_result(category, index, total_count, command, result)
                
                self.dfs.append([category, pd.DataFrame(data)])

            self.save_file()

    def count_result(self, category, result):
        if category not in self.results:
            self.results[category] = { "PASS": 0, "FAIL": 0, "OTHERS": 0 }

        if self.check_result_pass(result):
            self.results[category]["PASS"] += 1
        elif "fail" in result.lower():
            self.results[category]["FAIL"] += 1
        else:
            self.results[category]["OTHERS"] += 1
    
    def record_result(self, category, index, total_count, command, result):
        self.count_result(category, result)

        logger_string = f"[{category}({index + 1}/{total_count}) {command}] Result: '{result}'."

        if self.check_result_pass(result):
            logger.info(logger_string)
        else:
            logger.warning(logger_string)

    def append_result(self, data: dict[str, list], command, model, output, result: str):
        if self.check_result_pass(result):
            result = result.lower()
            for pass_result in self.pass_results:
                result = result.replace(pass_result, "PASS")

        data["Command"].append(command)
        data["Model"].append(model)
        data["Output"].append(output)
        data["Result"].append(result)

    def save_file(self):
        report = {
            "Category": [],
            "PASS": [],
            "FAIL": [],
            "OTHERS": [],
            "FailRate": []
        }

        for category in self.results:
            report["Category"].append(category)
            report["PASS"].append(self.results[category]["PASS"])
            report["FAIL"].append(self.results[category]["FAIL"])
            report["OTHERS"].append(self.results[category]["OTHERS"])
            report["FailRate"].append(self.results[category]["FAIL"] / (self.results[category]["PASS"] + self.results[category]["FAIL"] + self.results[category]["OTHERS"]))

        self.dfs.append(["Report", pd.DataFrame(report)])

        path = os.path.join(self.report_dir, self.report_file_name)
        with pd.ExcelWriter(path) as writer:
            for df in self.dfs:
                df[1].to_excel(writer, sheet_name=df[0], index=False)

        logger.info(f"Test result have been saved to '{path}'")

def get_config():
    config_path = "config.json"
    if not os.path.exists(config_path):
        logger.error(f"Config file '{config_path}' not exist.")
        sys.exit(1)
    
    try:
        with open(config_path, "r", encoding="utf-8") as f:
            return json.load(f)
    except Exception as e:
        logger.error(f"Failed to read Config file '{config_path}'.({e})")
        sys.exit(1)

def setup_logging():
    formatter = ColoredFormatter(
        '%(asctime)s %(log_color)s [%(levelname)s] %(message)s',
        datefmt='%Y-%m-%d %H:%M:%S',
        log_colors={
            'DEBUG': 'cyan',
            'INFO': 'white',
            'WARNING': 'yellow',
            'ERROR': 'red',
            'CRITICAL': 'bold_red',
        }
    )

    stream_handler = logging.StreamHandler()
    stream_handler.setLevel(logging.INFO)
    stream_handler.setFormatter(formatter)

    file_handler = logging.FileHandler(
        filename=".log", encoding="utf-8", mode="w")
    file_handler.setLevel(logging.INFO)
    file_handler.setFormatter(formatter)

    logging.basicConfig(
        handlers=[stream_handler, file_handler], level=logging.INFO
    )


if __name__ == "__main__":
    setup_logging()

    parser = argparse.ArgumentParser()
    parser.add_argument("--category", dest="category", help="test category")
    args = parser.parse_args()

    config = get_config()

    if args.category:
        input_categories = args.category.split(",")

        for category in input_categories:
            if category not in config["commands"]:
                logger.error(f"{category} is not the correct category, the category argument must be {', '.join(config["commands"])}.")
                sys.exit(1)
        
        categories = input_categories
    else:
        categories = list(config["commands"].keys())

    cli_script = CLIAutoTest(config, categories)
    cli_script.main()