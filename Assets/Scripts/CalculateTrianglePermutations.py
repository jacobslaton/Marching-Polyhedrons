def printList(printMe, prefix, indent):
	prefix = str(prefix)+(" " if prefix != [] else "")
	print(end=(indent)*"    "+prefix+"[\n")
	for pmi in printMe:
		print((indent+1)*"    "+str(pmi))
	print(end=(indent)*"    "+"]\n")

def CalculateTrianglePermutations(meshVertices, depth=0):
	if len(meshVertices) < 3:
		return []

	permutations = []
	fragment = list(meshVertices)
	fragment.pop(1)
	fragment.pop(0)

	for third in range(len(fragment)):
		newPermutations = []
		prefix = [[meshVertices[0], meshVertices[1], fragment[third]]]

		# Get Fragments
		firstFragment = [meshVertices[0]]+fragment[third:]
		secondFragment = [meshVertices[1]]+fragment[:third+1]
		if len(firstFragment) == 3:
			prefix += [firstFragment]
		if len(secondFragment) == 3:
			prefix += [secondFragment]

		print(end=(depth)*"    ")
		print("Prefix", prefix)
		print(end=(depth)*"    ")
		print("firstFragment", firstFragment)
		print(end=(depth)*"    ")
		print("secondFragment", secondFragment)

		# Check if fragments need to be reduced
		firstPermutations = []
		secondPermutations = []
		if len(firstFragment) > 3:
			firstPermutations += CalculateTrianglePermutations(firstFragment, depth+1)
		if len(secondFragment) > 3:
			secondPermutations += CalculateTrianglePermutations(secondFragment, depth+1)

		# Finalize new permutations
		if len(firstFragment) <= 3 and len(secondFragment) <= 3:
			newPermutations.append(prefix)
		elif len(firstFragment) > 3 and len(secondFragment) > 3:
			for fpi in range(len(firstPermutations)):
				for spi in range(len(secondPermutations)):
					print(prefix, fpi, spi)
					newPermutations.append(prefix+firstPermutations[fpi]+secondPermutations[spi])
		else:
			newPermutations += firstPermutations+secondPermutations
			for pi in range(len(newPermutations)):
				newPermutations[pi] = prefix+newPermutations[pi]
		permutations += newPermutations
		printList(permutations, "Permutations", depth)

	return permutations

# Clear the console
for ii in range(100):
	print()

meshVertices = (
	0, 1, 2, 3, 4, 5, 6
)
'''
	(0.5773503, -0.5773503, 0),
	(0, -0.5773503, -0.5773503),
	(-0.5773503, 0, -0.5773503),
	(-0.5773503, 0.5773503, 0),
	(0, 0.5773503, 0.5773503),
	(0.5773503, 0, 0.5773503),
'''

permutations = CalculateTrianglePermutations(meshVertices)

printList(permutations, len(permutations), 0)
