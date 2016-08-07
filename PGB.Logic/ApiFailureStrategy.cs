namespace PGB.Logic
{
    using System;
    using System.Threading.Tasks;

    using Logging;

    using PokemonGo.RocketAPI;
    using PokemonGo.RocketAPI.Exceptions;
    using PokemonGo.RocketAPI.Extensions;

    using POGOProtos.Networking.Envelopes;

    public class ApiFailureStrategy : IApiFailureStrategy
    {
        #region Constructors

        public ApiFailureStrategy()
        {
        }

        public ApiFailureStrategy(Client client)
        {
            Client = client;
        }

        #endregion

        #region Fields, properties, indexers and constants

        private int _retryCount;

        public Client Client { get; set; }

        #endregion

        #region IApiFailureStrategy Members

        public async Task<ApiOperation> HandleApiFailure(RequestEnvelope request, ResponseEnvelope response)
        {
            if (_retryCount == 11)
            {
                return ApiOperation.Abort;
            }

            await Task.Delay(500);
            _retryCount = _retryCount + 1;
            if (_retryCount%5 == 0)
            {
                var num = 0;
                try
                {
                    await DoLogin();
                }
                catch (PtcOfflineException ex)
                {
                    num = 1;
                }
                catch (AccessTokenExpiredException ex)
                {
                    num = 2;
                }
                catch (Exception ex) when (ex is InvalidResponseException || ex is TaskCanceledException)
                {
                    num = 3;
                }
                switch (num)
                {
                    case 1:
                        await Task.Delay(20000);
                        break;
                    case 2:
                        await Task.Delay(2000);
                        break;
                    case 3:
                        await Task.Delay(1000);
                        break;
                }
            }

            return ApiOperation.Retry;
        }

        public void HandleApiSuccess(RequestEnvelope request, ResponseEnvelope response)
        {
            _retryCount = 0;
        }

        #endregion

        #region Methods and other members

        private async Task DoLogin()
        {
            var num = 0;
            try
            {
                await Client.Login.DoLogin();
            }
            catch (AggregateException ex)
            {
                throw ex.Flatten().InnerException;
            }
            catch (LoginFailedException ex)
            {
                num = 1;
            }
            catch (AccessTokenExpiredException ex)
            {
                num = 2;
            }
            catch (PtcOfflineException ex)
            {
                num = 3;
            }
            catch (InvalidResponseException ex)
            {
                num = 4;
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }

            switch (num)
            {
                case 1:
                    Logger.Write("Login failed, the credentials you entered for your Pokemon GO account are invalid");
                    Logger.Write("Trying again in 1 second...");
                    await Task.Delay(1000);
                    break;
                case 2:
                    Logger.Write("Access token expired");
                    Logger.Write("Trying again in 1 second...");
                    await Task.Delay(1000);
                    break;
                case 3:
                    Logger.Write("PTC servers appear to be offline");
                    Logger.Write("Trying again in 15 seconds...");
                    await Task.Delay(15000);
                    break;
                case 4:
                    Logger.Write("Invalid response received from Niantic's servers");
                    Logger.Write("Trying again in 5 seconds...");
                    await Task.Delay(5000);
                    break;
            }
        }

        public async Task<ApiOperation> HandleApiFailure()
        {
            if (_retryCount == 11)
            {
                return ApiOperation.Abort;
            }

            await Task.Delay(500);
            _retryCount = _retryCount + 1;
            if (_retryCount%5 == 0)
            {
                await DoLogin();
            }
            return ApiOperation.Retry;
        }

        public void HandleApiSuccess()
        {
            _retryCount = 0;
        }

        #endregion
    }
}