# Battle Engine

## Movement

Movement in the UI is displayed as 1/2, 3/4, 1, 1 1/4, etc. It's actually stored as movement points divided over 4 round blocks, of which there are 4 of
those per battle (i.e. 16 rounds). A token's movement point is divided among the 4 round block, with movements being added in this order:

1, 3, 2, 4

i.e. A ship with 3 movement will move in the 1st, 3rd, and 2nd round, but not the fourth.

| movement/4 | 1   | 2   | 3   | 4   |
| ---------- | --- | --- | --- | --- |
| 2          | 1   | 0   | 1   | 0   |
| 3          | 1   | 1   | 0   | 1   |
| 4          | 1   | 1   | 1   | 1   |
| 5          | 2   | 1   | 1   | 1   |
| 6          | 2   | 1   | 2   | 1   |
| 7          | 2   | 2   | 1   | 2   |
| 8          | 2   | 2   | 2   | 2   |
| 9          | 3   | 2   | 2   | 2   |
| 10         | 3   | 2   | 3   | 2   |

In the battle engine, we will store the movement as an array of lists of tokens. Each token will be sorted by mass, and will add itself to the
table based on the 1, 3, 2, 4 movement allocation described above.

A design's movement is:

```
Movement = IdealEngineSpeed - 2 - Mass / 70 / NumEngines + NumManeuveringJets + 2*NumOverThrusters
```

A starting JoaT Stalwart Defender is 186kT with a Long Hump 6, so

`(6 - 2 - (186 / 70 / 1) = 4 - 2.6 = 1.4` (should be 2)

A starting JoaT Teamster is 130kT:

`(6 - 2 - (130 / 70 / 1) = 4 - 1.85 = 2.15` (should be 3)

Teamster with just long hump 6
`6 - 2 - 69 / 70 = 3.02` (should be 4)

add scanner to make mass 74kT
`6 - 2 - 74 / 70 = 
