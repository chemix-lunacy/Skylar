using Microsoft.ML;
using System.Collections.Generic;
using System.Linq;

namespace Skyling.ML
{
    public class TraitClassifier
    {
        public void TraitGeneration()
        {
            var mlContext = new MLContext();

                // input schema to the pipeline.
            var emptySamples = new List<TextData>();

            // Convert sample list to an empty IDataView.
            var emptyDataView = mlContext.Data.LoadFromEnumerable(emptySamples);

            // A pipeline for removing stop words from input text/string.
            // The pipeline first tokenizes text into words then removes stop words.
            // The 'RemoveStopWords' API ignores casing of the text/string e.g. 
            // 'tHe' and 'the' are considered the same stop words.
            string outputName = "Words";
            var textPipeline = mlContext.Transforms.Text.TokenizeIntoWords(outputName,
                nameof(TextData.Text))
                .Append(mlContext.Transforms.Text.RemoveDefaultStopWords(
                "WordsWithoutStopWords", outputName));

            // Fit to data.
            var textTransformer = textPipeline.Fit(emptyDataView);

            // Create the prediction engine to remove the stop words from the input
            // text /string.
            var predictionEngine = mlContext.Model.CreatePredictionEngine<TextData,
                TransformedTextData>(textTransformer);

            // Call the prediction API to remove stop words.
            var data = new TextData()
            {
                Text = "ML.NET's RemoveStopWords API " +
                "removes stop words from tHe text/string using a list of stop " +
                "words provided by the user."
            };

            var prediction = predictionEngine.Predict(data);

        }

        private class TextData
        {
            public string Text { get; set; }
        }

        private class TransformedTextData : TextData
        {
            public string[] WordsWithoutStopWords { get; set; }
        }
    }
}
