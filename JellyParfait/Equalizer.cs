using NAudio.Dsp;
using NAudio.Wave;

class Equalizer : ISampleProvider {
    private readonly ISampleProvider sourceProvider;
    private readonly EqualizerBand[] bands;
    private readonly BiQuadFilter[,] filters;
    private readonly int channels;
    private readonly int bandCount;
    private bool updated;

    public Equalizer(ISampleProvider sourceProvider, EqualizerBand[] bands) {
        this.sourceProvider = sourceProvider;
        this.bands = bands;
        channels = sourceProvider.WaveFormat.Channels;
        bandCount = bands.Length;
        filters = new BiQuadFilter[channels, bands.Length];
        CreateFilters();
    }

    private void CreateFilters() {
        for (int bandIndex = 0; bandIndex < bandCount; bandIndex++)
        {
            var band = bands[bandIndex];
            for (int i = 0; i < channels; i++)
            {
                if (filters[i, bandIndex] == null)
                    filters[i, bandIndex] = BiQuadFilter.PeakingEQ(sourceProvider.WaveFormat.SampleRate, band.Frequency, band.Bandwidth, band.Gain);
                else
                    filters[i, bandIndex].SetPeakingEq(sourceProvider.WaveFormat.SampleRate, band.Frequency, band.Bandwidth, band.Gain);
            }
        }
    }

    public void Update() {
        updated = true;
        CreateFilters();
    }
    //       public WaveFormat WaveFormat => sourceProvider.WaveFormat;
    public WaveFormat WaveFormat {
        get {
            return sourceProvider.WaveFormat;
        }
    }

    public int Read(float[] buffer, int offset, int count) {
        int samplesRead = sourceProvider.Read(buffer, offset, count);

        if (updated) {
            CreateFilters();
            updated = false;
        }

        for (int sample = 0; sample < samplesRead; sample++)
        {
            int ch = sample % channels;

            for (int band = 0; band < bandCount; band++)
            {
                buffer[offset + sample] = filters[ch, band].Transform(buffer[offset + sample]);
            }
        }
        return samplesRead;
    }
}