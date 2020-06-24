pipeline {
    agent {
        label 'windows'
    }

    options {
        skipDefaultCheckout(true)
    }

    environment {
        NUGET_APIKEY = credentials('nuget-api-key')
    }

    stages {
        stage('Checkout') {
            steps {
                deleteDir()
                checkout scm
            }
        }
        stage('Build') {
            steps {
                powershell './build.ps1 Compile -configuration Release -build-number $env:BUILD_NUMBER -branch-name $env:BRANCH_NAME'
            }
        }
        stage('Unit Test') {
            steps {
                powershell './build.ps1 Test -configuration Release -build-number $env:BUILD_NUMBER -branch-name $env:BRANCH_NAME --skip'
            }
        }
        stage('Package') {
            steps {
                powershell './build.ps1 Pack -configuration Release -build-number $env:BUILD_NUMBER -branch-name $env:BRANCH_NAME --skip'
            }
        }
        stage('Integration Test') {
            steps {
                powershell './build.ps1 IntegrationTest -configuration Release -build-number $env:BUILD_NUMBER -branch-name $env:BRANCH_NAME --skip'
            }
        }
        stage('Publish') {
            steps {
                powershell './build.ps1 Publish -nugetapikey $env:NUGET_APIKEY -configuration Release -build-number $env:BUILD_NUMBER -branch-name $env:BRANCH_NAME --skip'
            }
        }
        stage('Archive Artifacts') {
            steps {
                archiveArtifacts artifacts: 'artifacts/**'
            }
        }
    }

    post {
        always {
            nunit testResultsPattern: 'artifacts/**/*.xml'
        }
        failure {
            script {
                mail (
          to: 'eylvisaker@gmail.com',
          subject: "FAILED BUILD - AgateLib.ContentAssembler ${env.BRANCH_NAME} ${env.BUILD_NUMBER}",
          body: "Generations of Lore branch ${env.BRANCH_NAME} failed to build."
        )
            }
        }
        fixed {
            script {
                mail (
          to: 'eylvisaker@gmail.com',
          subject: "FIXED BUILD - AgateLib.ContentAssembler ${env.BRANCH_NAME} ${env.BUILD_NUMBER}",
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
