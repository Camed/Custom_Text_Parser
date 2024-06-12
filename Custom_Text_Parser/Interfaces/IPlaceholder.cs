using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Custom_Text_Parser.Interfaces;

public interface IPlaceholder
{
    string Name { get; }
    int Length { get; }
}
