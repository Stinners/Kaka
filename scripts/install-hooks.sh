#! sh

if !(test -f Kaka.sln)
then
    echo "ERROR: This script must be run from top level directory of solution"
    exit 1
fi

ln -sf ../../scripts/pre-commit.sh .git/hooks/pre-commit
