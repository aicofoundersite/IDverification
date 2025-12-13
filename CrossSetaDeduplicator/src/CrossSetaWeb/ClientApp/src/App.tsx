import { useState } from 'react'

function App() {
  const [searchId, setSearchId] = useState('')
  const [result, setResult] = useState<any>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const handleSearch = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!searchId) return

    setLoading(true)
    setError('')
    setResult(null)

    try {
      const response = await fetch(`/api/verification/home-affairs/${searchId}`)
      if (!response.ok) {
        throw new Error('Network response was not ok')
      }
      const data = await response.json()
      setResult(data)
    } catch (err) {
      setError('Failed to verify ID. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col items-center justify-center p-4">
      <div className="max-w-4xl w-full space-y-8 text-center">
        
        {/* Logo Section */}
        <div className="flex justify-center mb-8">
           <img src="/images/logo.jpg" alt="W&RSETA Logo" className="h-24" />
        </div>

        <h1 className="text-4xl font-extrabold text-gray-900 tracking-tight sm:text-5xl">
          Identity Verification Portal
        </h1>
        <p className="mt-4 text-xl text-gray-600">
          Secure, real-time learner verification and registration for W&RSETA.
        </p>

        {/* Action Buttons */}
        <div className="mt-10 flex justify-center gap-6">
          <a
            href="/Registration/User"
            className="px-8 py-4 bg-blue-600 text-white text-lg font-semibold rounded-lg shadow-md hover:bg-blue-700 transition-colors flex items-center gap-2"
          >
            <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z" />
            </svg>
            Register User
          </a>
          <a
            href="/Registration/Learner"
            className="px-8 py-4 bg-green-600 text-white text-lg font-semibold rounded-lg shadow-md hover:bg-green-700 transition-colors flex items-center gap-2"
          >
            <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
            </svg>
            Register Learner
          </a>
        </div>

        {/* Search Section */}
        <div className="mt-16 bg-white p-8 rounded-xl shadow-lg border border-gray-100 max-w-2xl mx-auto">
          <h2 className="text-2xl font-bold text-gray-800 mb-6">Quick Verification Check</h2>
          <form onSubmit={handleSearch} className="flex gap-4">
            <input
              type="text"
              placeholder="Enter South African ID Number"
              value={searchId}
              onChange={(e) => setSearchId(e.target.value)}
              className="flex-1 px-4 py-3 rounded-lg border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none transition-all"
            />
            <button
              type="submit"
              disabled={loading}
              className="px-6 py-3 bg-gray-800 text-white font-medium rounded-lg hover:bg-gray-900 transition-colors disabled:opacity-50"
            >
              {loading ? 'Checking...' : 'Verify ID'}
            </button>
          </form>

          {/* Results */}
          {error && (
            <div className="mt-6 p-4 bg-red-50 text-red-700 rounded-lg border border-red-100">
              {error}
            </div>
          )}

          {result && (
            <div className={`mt-6 p-6 rounded-lg border ${result.found ? 'bg-green-50 border-green-200' : 'bg-yellow-50 border-yellow-200'}`}>
              {result.found ? (
                <div className="text-left">
                  <div className="flex items-center gap-2 mb-4">
                    <div className="h-3 w-3 rounded-full bg-green-500 animate-pulse"></div>
                    <h3 className="text-lg font-bold text-green-800">Learner Found</h3>
                  </div>
                  <div className="grid grid-cols-2 gap-4 text-sm text-gray-700">
                    <div>
                      <span className="font-semibold block">Full Name:</span>
                      {result.data.firstName} {result.data.surname}
                    </div>
                    <div>
                      <span className="font-semibold block">Status:</span>
                      <span className={result.data.isDeceased ? "text-red-600 font-bold" : "text-green-600 font-bold"}>
                        {result.data.isDeceased ? 'Deceased' : 'Alive'}
                      </span>
                    </div>
                    <div>
                      <span className="font-semibold block">ID Number:</span>
                      {result.data.nationalID}
                    </div>
                    <div>
                      <span className="font-semibold block">Source:</span>
                      {result.data.verificationSource || 'Home Affairs'}
                    </div>
                  </div>
                </div>
              ) : (
                <div className="text-center text-yellow-800">
                  <p className="font-medium">No record found for this ID.</p>
                  <p className="text-sm mt-2">You can register this learner using the buttons above.</p>
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  )
}

export default App
