py_files=cat_bin.py requirements.txt
c_files=cat_bin.c 
flags=-g -Wall -Wextra
files=$(py_files) $(c_files) Makefile

build: $(c_files)
	gcc $(c_files) -o cat_bin $(flags)

dist: $(files)
	tar czvf bin_cat.tar.gz $(files)

clean:
	rm -f bin_cat.tar.gz cat_bin
