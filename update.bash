#!/bin/bash
rsync -rav --delete ../d2mpclient/D2MPClient/ ./d2mpclient/
rsync -rav --delete ../d2mpclient/D2MPClientInstaller/ ./d2mpclientinstaller/
rsync -rav --delete ../d2mpclient/ClientCommon/ ./ClientCommon/
