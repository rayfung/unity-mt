all: test_mt

test_mt: test_mt.c libmt.so
	gcc -O2 -Wall -o test_mt test_mt.c ./libmt.so

libmt.so: libmt.c
	gcc -c -O2 -fPIC -Wall -o libmt.o libmt.c
	gcc -shared -o libmt.so libmt.o -lX11 -lXi

clean:
	rm -f -- *.o *.so test_mt
