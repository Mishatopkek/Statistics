using System.Collections.Generic;

class ModeCalculator{
    private Dictionary<int, int> freq;
    private int mode, maxFreq;

    public ModeCalculator() {
        freq = new Dictionary<int, int>();
        mode = 0;
        maxFreq = 0;
    }

    public void Add(int num) {
        // update the frequency count of the element
        if (freq.ContainsKey(num)) {
            freq[num]++;
        } else {
            freq[num] = 1;
        }
        // update the mode and its frequency
        if (freq[num] > maxFreq) {
            mode = num;
            maxFreq = freq[num];
        }
    }

    public int GetMode()
    {
        var a = freq.Sum(x => x.Value);
        return mode;
    }
}