using System;

namespace SystemAudioRecordingSoftware.Domain.Types
{
    public class Result
    {
        protected Result(bool succeeded, string error)
        {
            if ((succeeded && !string.IsNullOrEmpty(error)) || (!succeeded && string.IsNullOrEmpty(error)))
            {
                throw new ArgumentException("Result must be either successful with empty error text or " +
                                            "failed with non-empty error text");
            }

            Succeeded = succeeded;
            ErrorText = error;
        }

        public bool Succeeded { get; private set; }
        public string ErrorText { get; private set; }

        public bool Failed
        {
            get { return !Succeeded; }
        }

        public static Result Combine(params Result[] results)
        {
            foreach (Result result in results)
            {
                if (result.Failed)
                    return result;
            }

            return Success();
        }

        public static Result Error(string message)
        {
            return new Result(false, message);
        }

        public static Result<T> Error<T>(string message)
        {
            return new(default, false, message);
        }

        public static Result Success()
        {
            return new(true, string.Empty);
        }

        public static Result<T> Success<T>(T value)
        {
            return new(value, true, string.Empty);
        }
    }


    public class Result<T> : Result
    {
        private T? _value;

        protected internal Result(T? value, bool succeeded, string error)
            : base(succeeded, error)
        {
            if (succeeded && value == null)
            {
                throw new ArgumentException("Value cannot be null for a succeeded result");
            }

            Value = value;
        }

        public T? Value
        {
            get
            {
                if (!Succeeded)
                {
                    throw new InvalidOperationException("Cannot get value of a failed result");
                }

                return _value;
            }
            private set { _value = value; }
        }
    }
}