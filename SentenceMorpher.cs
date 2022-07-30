using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Morphology
{
    public class SentenceMorpher
    {
        /// <summary>
        ///     Создает <see cref="SentenceMorpher"/> из переданного набора строк словаря.
        /// </summary>
        /// <remarks>
        ///     В этом методе должен быть код инициализации: 
        ///     чтение и преобразование входных данных для дальнейшего их использования
        /// </remarks>
        /// <param name="dictionaryLines">
        ///     Строки исходного словаря OpenCorpora в формате plain-text.
        ///     <code> СЛОВО(знак_табуляции)ЧАСТЬ РЕЧИ( )атрибут1[, ]атрибут2[, ]атрибутN </code>
        /// </param>
        /// 

        public Dictionary<string, List<ValueTuple<List<string>, string>>> morphes2;
        public static SentenceMorpher Create(IEnumerable<string> dictionaryLines)
        {
            var morphes = new Dictionary<string, List<ValueTuple<List<string>, string>>>();
            bool keyBool = false;
            string baseWord = "";
            foreach (var item in dictionaryLines)
            {
                if (item == "") continue;
                var item1 = item.Trim()[0];
                if (Char.IsDigit(item1))
                {
                    keyBool = true;
                    continue;
                }

                var itemSplit = item.Split(new[] { '\t', ' ' }, 2);

                if (keyBool)
                {
                    baseWord = itemSplit[0].ToLower();

                    if (!morphes.ContainsKey(baseWord))
                        morphes.Add(baseWord, new List<ValueTuple<List<string>, string>>());

                    keyBool = false;
                }

                var convertedWord = itemSplit[0];
                var listAttribute = itemSplit[1].ToLower().Split(' ', ',').ToList();

                if (morphes.ContainsKey(baseWord))
                    morphes[baseWord].Add((listAttribute, convertedWord));
            }

            return new SentenceMorpher() { morphes2 = morphes };
        }

        /// <summary>
        ///     Выполняет склонение предложения согласно указанному формату
        /// </summary>
        /// <param name="sentence">
        ///     Входное предложение <para/>
        ///     Формат: набор слов, разделенных пробелами.
        ///     После слова может следовать спецификатор требуемой части речи (формат описан далее),
        ///     если он отсутствует - слово требуется перенести в выходное предложение без изменений.
        ///     Спецификатор имеет следующий формат: <code>{ЧАСТЬ РЕЧИ,аттрибут1,аттрибут2,..,аттрибутN}</code>
        ///     Если для спецификации найдётся несколько совпадений - используется первое из них
        /// </param>
        public virtual string Morph(string sentence)
        {
            if (string.IsNullOrWhiteSpace(sentence)) return "";
            string result = "";
            var sentenceSplit = sentence.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            foreach (var lineSearch in sentenceSplit)
            {
                var searchAttributeKey = GetValueTuple(lineSearch);
                var key = searchAttributeKey.Item2;
                var searchKey = key.ToLower();
                var searchAttribute = searchAttributeKey.Item1;
                List<ValueTuple<List<string>, string>> morphesBaseWord;

                if (morphes2.ContainsKey(searchKey))
                {
                    morphesBaseWord = morphes2[searchKey];

                    foreach (var item in searchAttribute)
                    {
                        morphesBaseWord = morphesBaseWord
                            .Where(x => x.Item1.Contains(item))
                            .ToList();
                    }

                    if (morphesBaseWord.Count > 0)
                        result += " " + morphesBaseWord.First().Item2;
                    else
                        result += " " + key;
                }
                else
                {
                    result += " " + key;
                }
            }

            return result.Trim();
        }

        public (List<string>, string) GetValueTuple(string line)
        {
            string convertedWord;
            ;
            List<string> listAttribute;

            if (!line.Contains('{'))
                return (new List<string>(), line.Trim());

            var wordAttribute = line.Split("{");
            convertedWord = wordAttribute[0];
            listAttribute = wordAttribute[1].Trim('}').ToLower().Split(',').ToList();
            return (listAttribute, convertedWord);
        }
    }
}