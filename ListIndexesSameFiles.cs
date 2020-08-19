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
        public List<int> ListIndexes;   // список с индексами одинаковых файлов

        public void AddSourceIndex(int AddIndex, bool NoPair)
        {
            int count = ((AmountIndexes == 0) ? 1 : 0) + (NoPair ? 1 : 0); // записываем обязательно первый встречающийся элемент и добавляем элемент, у которого нет пары
            for (int c = 1; c <= count; c++)
            {
                AmountIndexes++;
                if (AmountIndexes > ListIndexes.Count)
                    ListIndexes.Add(AddIndex);
                else
                    ListIndexes[AmountIndexes] = AddIndex;
            }
        }

        public void ResetListIndexesSameFiles()
        {
            AmountIndexes = 0;
        }
    }
}
