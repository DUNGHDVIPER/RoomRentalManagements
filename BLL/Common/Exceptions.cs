
namespace BLL.Common
{
    internal class Exceptions
    {
        [Serializable]
        internal class BusinessException : Exception
        {
            public BusinessException()
            {
            }

            public BusinessException(string? message) : base(message)
            {
            }

            public BusinessException(string? message, Exception? innerException) : base(message, innerException)
            {
            }
        }

        [Serializable]
        internal class NotFoundException : Exception
        {
            public NotFoundException()
            {
            }

            public NotFoundException(string? message) : base(message)
            {
            }

            public NotFoundException(string? message, Exception? innerException) : base(message, innerException)
            {
            }
        }
    }
}