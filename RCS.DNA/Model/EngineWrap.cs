using RCS.Carbon.Variables;
using RCS.Licensing.Provider.Shared;

namespace RCS.DNA.Model;

internal class EngineWrap : IDisposable
{
	readonly VEngine _engine;
	bool _open;
	readonly AuthenticateSequence _authSequence;
	readonly string _idOrName;
	readonly string _password;
	readonly string _customerName;
	readonly string _jobName;

	public static async Task<EngineWrap> CreateAsync(ILicensingProvider provider, AuthenticateSequence authSequence, string idOrName, string password, string customerName, string jobName)
	{
		var engine = new EngineWrap(provider, authSequence, idOrName, password, customerName, jobName);
		await engine.Authenticate();
		engine.OpenJob();
		return engine;
	}

	EngineWrap(ILicensingProvider provider, AuthenticateSequence authSequence, string idOrName, string password, string customerName, string jobName)
	{
		_authSequence = authSequence;
		_idOrName = idOrName;
		_password = password;
		_customerName = customerName;
		_jobName = jobName;
		_engine = new VEngine(provider);
	}

	public void Dispose()
	{
		if (_open)
		{
			_engine.CloseJob();
		}
	}

	public VEngine Engine => _engine;

	async Task Authenticate()
	{
		async Task<LicenceInfo?> NameAuthenticate(bool ignoreFail)
		{
			try
			{
				return await _engine.GetLicenceName(_idOrName!, _password!);
			}
			catch
			{
				if (ignoreFail) return null;
				throw;
			}
		}
		async Task<LicenceInfo?> IdAuthenticate(bool ignoreFail)
		{
			try
			{
				return await _engine.GetLicenceId(_idOrName!, _password!, true);
			}
			catch
			{
				if (ignoreFail) return null;
				throw;
			}
		}
		if (_authSequence == AuthenticateSequence.NameOnly)
		{
			await NameAuthenticate(false);
		}
		else if (_authSequence == AuthenticateSequence.IdOnly)
		{
			await IdAuthenticate(false);
		}
		else if (_authSequence == AuthenticateSequence.NameThenId)
		{
			var licinfo = await NameAuthenticate(true);
			if (licinfo != null) return;
			await IdAuthenticate(false);
		}
		else if (_authSequence == AuthenticateSequence.IdThenName)
		{
			var licinfo = await IdAuthenticate(true);
			if (licinfo != null) return;
			await NameAuthenticate(false);
		}
	}

	void OpenJob()
	{
		_engine.OpenJob(_customerName, _jobName);
		_open = true;
	}

}
