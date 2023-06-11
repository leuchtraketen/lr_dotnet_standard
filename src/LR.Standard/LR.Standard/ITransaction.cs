using System;
using System.Collections.Generic;
using System.Text;

namespace LR.Standard;

public interface ITransaction
{
    void Commit();
    void Rollback();
}
