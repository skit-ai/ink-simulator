BINS = InkSimulator.exe

all: $(BINS)

.PHONY: clean
clean:
	rm $(BINS)

DLLS = ./lib/ink-engine-runtime.dll ./lib/ink_compiler.dll
REFS = -reference:./lib/ink-engine-runtime.dll -reference:./lib/ink_compiler.dll
SRCS = ./src/InkSimulator.cs ./src/InkFlowCompiler.cs

InkSimulator.exe: $(SRCS) $(DLLS)
	csc $(REFS) $(SRCS)
