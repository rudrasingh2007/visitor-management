$ErrorActionPreference = 'Stop'
$baseUrl = "http://localhost:5000"
$cookieJar = New-Object System.Net.CookieContainer

function Login-User {
    param($email, $password)
    $response = Invoke-WebRequest -Uri "$baseUrl/Account/Login" -SessionVariable global:session -UseBasicParsing
    $requestToken = ($response.Content | Select-String -Pattern '<input name="__RequestVerificationToken" type="hidden" value="([^"]+)"' | % { $_.Matches.Groups[1].Value })
    
    $body = @{
        "Email" = $email
        "Password" = $password
        "__RequestVerificationToken" = $requestToken
    }
    
    $loginResponse = Invoke-WebRequest -Uri "$baseUrl/Account/Login" -Method Post -Body $body -WebSession $global:session -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
    if ($loginResponse.StatusCode -eq 302) {
        Write-Host "Logged in as $email successfully" -ForegroundColor Green
    } else {
        Write-Host "Failed to login as $email" -ForegroundColor Red
    }
}

# 1. Login as Admin to create an Entry Request
Login-User "admin@vms.com" "Admin@123"

# 2. Get Create Entry Request form
$response = Invoke-WebRequest -Uri "$baseUrl/EntryRequest/Create" -WebSession $global:session -UseBasicParsing
$requestToken = ($response.Content | Select-String -Pattern '<input name="__RequestVerificationToken" type="hidden" value="([^"]+)"' | % { $_.Matches.Groups[1].Value })

$body = @{
    "VisitorId" = "4"
    "DepartmentId" = "2"
    "EmployeeId" = "4"
    "Purpose" = "Full Testing Workflow"
    "__RequestVerificationToken" = $requestToken
}

$createResponse = Invoke-WebRequest -Uri "$baseUrl/EntryRequest/Create" -Method Post -Body $body -WebSession $global:session -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
if ($createResponse.StatusCode -eq 302) {
    Write-Host "Entry Request Created Successfully" -ForegroundColor Green
}

# Fetch the created Entry Request ID from DB
$env:PGPASSWORD="pgsql"
$entryIdStr = & "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -h localhost -p 5432 -d visitor_management -t -c "SELECT ""EntryRequestId"" FROM ""EntryRequestMaster"" ORDER BY ""EntryRequestId"" DESC LIMIT 1;"
$entryId = $entryIdStr.Trim()
Write-Host "Created Entry Request ID: $entryId"

# 3. Login as Employee to Approve
Login-User "bob@vms.com" "Bob@123"

$response = Invoke-WebRequest -Uri "$baseUrl/EntryRequest/Approve/$entryId" -WebSession $global:session -UseBasicParsing
$requestToken = ($response.Content | Select-String -Pattern '<input name="__RequestVerificationToken" type="hidden" value="([^"]+)"' | % { $_.Matches.Groups[1].Value })

$body = @{
    "ApprovalRemarks" = "Approved for full testing"
    "__RequestVerificationToken" = $requestToken
}

$approveResponse = Invoke-WebRequest -Uri "$baseUrl/EntryRequest/Approve/$entryId" -Method Post -Body $body -WebSession $global:session -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
if ($approveResponse.StatusCode -eq 302) {
    Write-Host "Entry Request Approved Successfully" -ForegroundColor Green
}

# 4. Login as Security Guard to Generate Pass
Login-User "sg1@vms.com" "SG1@123"

$response = Invoke-WebRequest -Uri "$baseUrl/GatePass/Generate?entryRequestId=$entryId" -WebSession $global:session -UseBasicParsing
$requestToken = ($response.Content | Select-String -Pattern '<input name="__RequestVerificationToken" type="hidden" value="([^"]+)"' | % { $_.Matches.Groups[1].Value })

$body = @{
    "appointmentId" = ""
    "entryRequestId" = $entryId
    "photoBase64" = ""
    "__RequestVerificationToken" = $requestToken
}

$generateResponse = Invoke-WebRequest -Uri "$baseUrl/GatePass/Generate" -Method Post -Body $body -WebSession $global:session -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
if ($generateResponse.StatusCode -eq 302) {
    Write-Host "Gate Pass Generated Successfully" -ForegroundColor Green
}

# Try generating pass AGAIN (Should fail)
$generateResponse2 = Invoke-WebRequest -Uri "$baseUrl/GatePass/Generate" -Method Post -Body $body -WebSession $global:session -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
if ($generateResponse2.StatusCode -eq 302) {
    Write-Host "Duplication check failed? Redirected" -ForegroundColor Yellow
} else {
    Write-Host "Duplicate Gate Pass prevented properly" -ForegroundColor Green
}

# Fetch the Visit Entry ID
$visitIdStr = & "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -h localhost -p 5432 -d visitor_management -t -c "SELECT ""VisitEntryId"" FROM ""VisitEntryMaster"" WHERE ""EntryRequestId"" = $entryId;"
$visitId = $visitIdStr.Trim()
Write-Host "Visit Entry ID: $visitId"

# 5. Guard Checks Out Visitor
$response = Invoke-WebRequest -Uri "$baseUrl/Visit" -WebSession $global:session -UseBasicParsing
$requestToken = ($response.Content | Select-String -Pattern '<input name="__RequestVerificationToken" type="hidden" value="([^"]+)"' | % { $_.Matches.Groups[1].Value })

$body = @{
    "id" = $visitId
    "checkoutRemarks" = "Checked out after full test"
    "__RequestVerificationToken" = $requestToken
}

$checkoutResponse = Invoke-WebRequest -Uri "$baseUrl/Visit/CheckOut/$visitId" -Method Post -Body $body -WebSession $global:session -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
if ($checkoutResponse.StatusCode -eq 302) {
    Write-Host "Visitor Checked Out Successfully" -ForegroundColor Green
}

# 6. Verify PrintPass page output
$printResponse = Invoke-WebRequest -Uri "$baseUrl/Visit/PrintPass/$visitId" -WebSession $global:session -UseBasicParsing
if ($printResponse.Content -match "Print Pass Now") {
    Write-Host "BUG: Print button is still visible for checked out visitor!" -ForegroundColor Red
} else {
    Write-Host "Print button is correctly hidden for checked out visitor." -ForegroundColor Green
}

Write-Host "All tests completed." -ForegroundColor Cyan
