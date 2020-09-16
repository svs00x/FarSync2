using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarSync2
{
    class ListIndexesSameFiles
    {
        public int AmountIndexes = 0;   // количество текущих индексов для одинаковых файлов, первый индекс только если не встретились другие файлы для перестановок
        public List<int> ListIndexes = new List<int>();   // список с индексами одинаковых файлов

        // добавляет элемент для связывания в пары. Если ещё не было элементов, то добавляется нулевой элемент
        // Параметры: добавляемый индекс; true - надо добавить элемент для пары, false - только обозначить присутствие элемента в приёмнике
        public void AddIndex(int AddingIndex, bool AddPair)
        {
            for (int c = 1; c < 3; c++)
            {
                if ( (c == 1 && AmountIndexes == 0)
                  || (c == 2 && AddPair) )
                {
                    if(AmountIndexes == ListIndexes.Count)
                        ListIndexes.Add(0);
                    ListIndexes[AmountIndexes] = AddingIndex;
                    AmountIndexes++;
                }
            }
        }

        public void ResetListIndexesSameFiles()
        {
            AmountIndexes = 0;
        }
    }
}
