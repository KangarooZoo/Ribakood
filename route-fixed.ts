import { type NextRequest, NextResponse } from "next/server"

import { getLicense, addMachineToLicense } from "@/lib/licenses"



export interface ValidateKeyRequest {

  Key: string

  MachineId: string

  AppVersion: string

}



export interface ValidateKeyResponse {

  IsValid: boolean

  ExpiryDate: string

  StatusMessage: string

}



export async function POST(request: NextRequest) {

  try {

    const body: ValidateKeyRequest = await request.json()

    console.log("[v0] Received license validation request:", {
      Key: body.Key,
      MachineId: body.MachineId?.substring(0, 20) + "...",
      AppVersion: body.AppVersion
    })



    // Validate input

    if (!body.Key || !body.MachineId) {

      console.log("[v0] Validation failed: Missing required fields")

      return NextResponse.json(

        {

          IsValid: false,

          ExpiryDate: "0001-01-01T00:00:00Z",

          StatusMessage: "Missing required fields: Key and MachineId are required",

        } as ValidateKeyResponse,

        { status: 400 },

      )

    }



    const license = await getLicense(body.Key)
    console.log("[v0] License lookup result:", license ? "Found" : "Not found", body.Key)
    if (license) {
      console.log("[v0] License details:", {
        id: license.id,
        license_key: license.license_key,
        is_valid: license.is_valid,
        expiry_date: license.expiry_date?.toISOString(),
        max_machines: license.max_machines,
        machines_count: license.machines?.length ?? 0
      })
    }



    // Check if license exists

    if (!license) {

      console.log("[v0] License not found for key:", body.Key)

      return NextResponse.json({

        IsValid: false,

        ExpiryDate: "0001-01-01T00:00:00Z",

        StatusMessage: "Invalid license key",

      } as ValidateKeyResponse)

    }



    // Extract license properties - getLicense returns snake_case with expiry_date as Date
    const isValid = license.is_valid
    const expiryDate = license.expiry_date // Already a Date object from getLicense
    const machines = license.machines ?? [] // Array of machine IDs
    const maxMachines = license.max_machines

    console.log("[v0] Normalized license:", {
      isValid,
      expiryDate: expiryDate?.toISOString(),
      machinesCount: machines.length,
      maxMachines
    })

    // Check if license is manually deactivated

    if (!isValid) {

      console.log("[v0] License is deactivated:", body.Key)

      return NextResponse.json({

        IsValid: false,

        ExpiryDate: expiryDate?.toISOString() || "0001-01-01T00:00:00Z",

        StatusMessage: "License has been deactivated",

      } as ValidateKeyResponse)

    }



    // Check if expired

    const now = new Date()

    if (!expiryDate) {

      console.log("[v0] License has no expiry date:", body.Key)

      return NextResponse.json({

        IsValid: false,

        ExpiryDate: "0001-01-01T00:00:00Z",

        StatusMessage: "License has no expiry date",

      } as ValidateKeyResponse)

    }

    if (now > expiryDate) {

      console.log("[v0] License expired:", body.Key, "Expired:", expiryDate, "Now:", now)

      return NextResponse.json({

        IsValid: false,

        ExpiryDate: expiryDate.toISOString(),

        StatusMessage: `License expired on ${expiryDate.toLocaleDateString()}`,

      } as ValidateKeyResponse)

    }



    // Check machine binding

    if (!machines.includes(body.MachineId)) {

      if (machines.length >= maxMachines) {

        return NextResponse.json({

          IsValid: false,

          ExpiryDate: expiryDate.toISOString(),

          StatusMessage: `License already activated on maximum number of machines (${maxMachines})`,

        } as ValidateKeyResponse)

      }



      // Add machine to license and handle potential errors

      try {

        await addMachineToLicense(body.Key, body.MachineId)

        // Refresh license after adding machine to ensure we have latest data

        const updatedLicense = await getLicense(body.Key)

        if (updatedLicense) {

          return NextResponse.json({

            IsValid: true,

            ExpiryDate: updatedLicense.expiryDate.toISOString(),

            StatusMessage: "License activated successfully",

          } as ValidateKeyResponse)

        }

      } catch (error) {

        console.error("[v0] Error adding machine to license:", error)

        return NextResponse.json({

          IsValid: false,

          ExpiryDate: expiryDate?.toISOString() || "0001-01-01T00:00:00Z",

          StatusMessage: "Failed to activate license on this machine",

        } as ValidateKeyResponse, { status: 500 })

      }

    }



    // Success - machine already registered or successfully added

    console.log("[v0] License validation successful:", body.Key, "for machine:", body.MachineId?.substring(0, 20) + "...")

    if (!expiryDate) {

      console.error("[v0] License has no expiry date but passed validation:", body.Key)

      return NextResponse.json({

        IsValid: false,

        ExpiryDate: "0001-01-01T00:00:00Z",

        StatusMessage: "License data is invalid",

      } as ValidateKeyResponse, { status: 500 })

    }

    return NextResponse.json({

      IsValid: true,

      ExpiryDate: expiryDate.toISOString(),

      StatusMessage: "License activated successfully",

    } as ValidateKeyResponse)

  } catch (error) {

    console.error("[v0] License validation error:", error)

    return NextResponse.json(

      {

        IsValid: false,

        ExpiryDate: "0001-01-01T00:00:00Z",

        StatusMessage: "Internal server error",

      } as ValidateKeyResponse,

      { status: 500 },

    )

  }

}

