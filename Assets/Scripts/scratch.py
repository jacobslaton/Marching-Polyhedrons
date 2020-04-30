import math

# Clear the console
for ii in range(100):
	print()

vertexObjects = [False]*8
cases = 1 << len(vertexObjects)
for ii in range(cases):
	vertexObjects[0] = True

	for jj in range(len(vertexObjects)):
		vertexObjects[jj] = ii & (1 << jj) == 1 << jj

	print(ii, vertexObjects)
