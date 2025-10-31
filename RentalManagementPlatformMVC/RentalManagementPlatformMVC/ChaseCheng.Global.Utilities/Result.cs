using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaseCheng.Global.Utilities
{
	public class Result
	{
		bool _isSuccess;
		string _message = string.Empty;

		Result() { }

		public static Result Success() => new Result { _isSuccess = true };
		public static Result Fail(string message) => new Result { _isSuccess = false, _message = message };
	}
}
