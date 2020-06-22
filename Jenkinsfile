pipeline {
  agent { 
    label 'windows' 
  }

  options {
    skipDefaultCheckout(true)
  }

  stages {
    stage('Checkout') {
      steps {
        deleteDir()
        checkout scm
      }
    }
    stage('Build for Windows') {
      steps {
        echo 'Building Windows...'
        powershell './build.ps1 Compile -stampversion -targets Windows -configuration Release_WindowsDX -build-number $env:BUILD_NUMBER -branch-name $env:BRANCH_NAME'
      }
    }
    stage('Build for Linux') {
      steps {
        echo 'Building...'
        powershell './build.ps1 Compile -stampversion -targets Linux -configuration Release_DesktopGL -build-number $env:BUILD_NUMBER -branch-name $env:BRANCH_NAME'
      }
    }
    stage('Build for Mac OS') {
      steps {
        echo 'Building...'
        powershell './build.ps1 Compile -stampversion -targets MacOS -configuration Release_DesktopGL -build-number $env:BUILD_NUMBER -branch-name $env:BRANCH_NAME'
      }
    }
    stage('Test') {
      steps {
        echo 'Testing...'
        powershell './build.ps1 Test -stampversion -configuration Release_Test -build-number $env:BUILD_NUMBER -branch-name $env:BRANCH_NAME --skip'
      }
    }
    stage('Package') {
      steps {
        echo 'Packaging...'
        powershell './build.ps1 Package -stampversion -targets Windows,Linux,MacOS -configuration Release_WindowsDX -build-number $env:BUILD_NUMBER -branch-name $env:BRANCH_NAME --skip'
        powershell './build.ps1 Package -stampversion -targets Linux,MacOS -configuration Release_DesktopGL -build-number $env:BUILD_NUMBER -branch-name $env:BRANCH_NAME --skip'
      }
    }
    stage('Docs') {
      steps {
        echo 'Building documentation...'
        powershell './build.ps1 Docs -stampversion -build-number $env:BUILD_NUMBER -branch-name $env:BRANCH_NAME'
      }
    }
    stage('Archive Artifacts') {
      steps {
        archiveArtifacts artifacts: 'artifacts/**'
      }
    }
    stage('Publish') {
      steps {
        echo 'Archiving artifacts...'
        powershell 'Copy-Item -Path Artifacts/* -Destination //blacksilver/Artifacts/GenerationsOfLore/ -Recurse -Force; if ($error) { $error; exit 1 }'
      }
    }
  }

  post {
    always {
      nunit testResultsPattern: 'artifacts/**/tests/*.xml'
    }
    failure {
      script {
        mail (
          to: 'eylvisaker@gmail.com',
          subject: "FAILED BUILD - GenLore ${env.BRANCH_NAME} ${env.BUILD_NUMBER}",
          body: "Generations of Lore branch ${env.BRANCH_NAME} failed to build."
        )
      }
    }
    fixed {
      script {
        mail (
          to: 'eylvisaker@gmail.com', 
          subject: "FIXED BUILD - GenLore ${env.BRANCH_NAME} ${env.BUILD_NUMBER}",
          body: "Generations of Lore branch ${env.BRANCH_NAME} is fixed now."
        )
      }
    }
    success {
      script {
        cleanWs()
      }
    }
  }
}
