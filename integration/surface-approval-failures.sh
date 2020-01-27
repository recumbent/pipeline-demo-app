#!/usr/bin/env bash
set -e

echo "Copying failed approvals (if any)"

rm -rf failed-approvals

# So there is always something to copy back, even if its empty
mkdir failed-approvals

receivedFiles=$(find ./*/*/approvals -type f -name "*.received.*")
for f in $receivedFiles;
do
    received=$f
    approved=${f/.received./.approved.}

    # Remove approvals from target path
    a="/approvals/"
    b="/"
    rec1=${received/$a/$b}

    # Target differ folder
    c="integration/suites"
    d="failed-approvals"
    rec2=${rec1/$c/$d}

    # Get the target folder name
    target=`dirname $rec2`

    # Create the folder (whole path)
    mkdir -p $target

    # Copy the received file and the approved file (if one exists)
    cp $received $target
    if [ -e $approved ]
        then cp -f $approved $target
    fi
done

