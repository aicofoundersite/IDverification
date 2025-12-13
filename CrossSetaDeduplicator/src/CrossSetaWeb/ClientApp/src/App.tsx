function App() {
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
            className="px-8 py-4 bg-[#2E8B57] text-white text-lg font-semibold rounded-lg shadow-md hover:bg-[#246D43] transition-colors flex items-center gap-2"
          >
            <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z" />
            </svg>
            Register User
          </a>
          <a
            href="/Registration/Learner"
            className="px-8 py-4 bg-[#2E8B57] text-white text-lg font-semibold rounded-lg shadow-md hover:bg-[#246D43] transition-colors flex items-center gap-2"
          >
            <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
            </svg>
            Register Learner
          </a>
          <a
            href="/Registration/Bulk"
            className="px-8 py-4 bg-[#2E8B57] text-white text-lg font-semibold rounded-lg shadow-md hover:bg-[#246D43] transition-colors flex items-center gap-2"
          >
            <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12" />
            </svg>
            Identity Verification
          </a>
        </div>
      </div>
    </div>
  )
}

export default App
