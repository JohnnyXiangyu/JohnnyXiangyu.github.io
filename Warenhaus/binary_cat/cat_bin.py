import argparse

parser = argparse.ArgumentParser()
parser.add_argument("filename", type=str, help='Enter the file you want to convert.')
args = parser.parse_args()

file_out = ''

file_in = open(args.filename, mode='rb')
for line in file_in:
    for char in line:
        file_out = file_out + str(int(char)) + ' '

print(file_out)
