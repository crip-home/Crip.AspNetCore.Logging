# AspNetCore Logging long json content middleware

![issues](https://img.shields.io/github/issues/crip-home/Crip.AspNetCore.Logging?style=for-the-badge&logo=appveyor)
![forks](https://img.shields.io/github/forks/crip-home/Crip.AspNetCore.Logging?style=for-the-badge&logo=appveyor)
![stars](https://img.shields.io/github/stars/crip-home/Crip.AspNetCore.Logging?style=for-the-badge&logo=appveyor)
![license](https://img.shields.io/github/license/crip-home/Crip.AspNetCore.Logging?style=for-the-badge&logo=appveyor)

`LongJsonContentMiddleware` implements `IRequestContentLogMiddleware` from **Crip.AspNetCore.Logging**.  
This implementation will hide properties value if its length exceeds 500 characters and will output only first 10
symbols:

```json
{
  "fileBase64": "SGVsbG8gV2***"
}
```

This middleware runs only if request content type is `application/json`. You can change this middleware values within
configuration on initialization:

```cs
services.AddRequestLogging(options =>
{
    options.LongJsonContent.MaxCharCountInField = 255;
    options.LongJsonContent.LeaveOnTrimCharCountInField = 15;
});
```
