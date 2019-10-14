BINS = ink-simulator.exe

all: $(BINS)

.PHONY: clean
clean:
	rm $(BINS)

ink-simulator.exe: ./src/InkSimulator.cs ./lib/ink-engine-runtime.dll
	csc -reference:./lib/ink-engine-runtime.dll ./src/InkSimulator.cs
