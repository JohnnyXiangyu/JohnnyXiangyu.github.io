#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <fcntl.h>

int in_file = STDIN_FILENO; // pointer to file object
int file_flag = 0; // whether file is used as input

unsigned char buffer[1024]; // buffer for reading characters

int main(int argc, char* argv[]) {
    if (argc == 2) {
        file_flag = 1;
        in_file = open(argv[1], O_RDONLY);
    }
    else if (argc > 2) {
        fprintf(stderr, "ERROR: too many arguments\n");
        exit(114);
    }

    int count = 0;
    while (1) {
        count = read(in_file, buffer, 1024);
        if (count <= 0) { break; }
        int i;
        for (i = 0; i < count; i++) {
            printf("%d ", buffer[i]);
        }
    }

    printf("\n");

    if (file_flag) { close(in_file); }

    return 0;
}